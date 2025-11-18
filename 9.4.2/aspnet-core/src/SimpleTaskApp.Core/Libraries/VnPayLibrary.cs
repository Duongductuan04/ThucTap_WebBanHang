using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using SimpleTaskApp.Vnpay; // để dùng PaymentResponseModel

namespace SimpleTaskApp.Libraries
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        #region Request/Response Data Handling
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }
        #endregion

        #region Build Payment URL
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();
            if (querystring.EndsWith("&"))
            {
                querystring = querystring.Remove(querystring.Length - 1, 1);
            }

            var signData = querystring;
            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);

            return $"{baseUrl}?{querystring}&vnp_SecureHash={vnpSecureHash}";
        }
        #endregion

        #region Response Validation
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    AddResponseData(key, value);
                }
            }

            var orderId = GetResponseData("vnp_TxnRef");
            var vnPayTranId = GetResponseData("vnp_TransactionNo");
            var vnpResponseCode = GetResponseData("vnp_ResponseCode");
            var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var orderInfo = GetResponseData("vnp_OrderInfo");

            var checkSignature = ValidateSignature(vnpSecureHash, hashSecret);
            if (!checkSignature)
            {
                return new PaymentResponseModel
                {
                    Success = false
                };
            }

            return new PaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = orderInfo,
                OrderId = orderId,
                PaymentId = vnPayTranId,
                TransactionId = vnPayTranId,
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetRawResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string GetRawResponseData()
        {
            var data = new StringBuilder();

            // loại bỏ hash trước khi tạo chuỗi
            _responseData.Remove("vnp_SecureHashType");
            _responseData.Remove("vnp_SecureHash");

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
        #endregion

        #region Utils
        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (remoteIpAddress == null) return "127.0.0.1";

                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }

                return remoteIpAddress?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        #endregion
    }
}
public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}
