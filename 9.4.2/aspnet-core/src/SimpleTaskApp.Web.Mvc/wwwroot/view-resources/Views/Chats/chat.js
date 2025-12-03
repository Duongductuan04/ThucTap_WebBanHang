// =========================
// Toggle chat popup
// =========================
function toggleChat() {
  const popup = document.getElementById("chatPopup");
  const icon = document.getElementById("chatIcon");

  const isHidden = popup.style.display === "none" || popup.style.display === "";

  if (isHidden) {
    popup.style.display = "flex";
    icon.style.display = "none";

    loadHistory(); // Load lịch sử khi mở popup

    document.getElementById("chatInput").focus();
  } else {
    popup.style.display = "none";
    icon.style.display = "flex";
  }
}

// =========================
// Click events
// =========================
document.getElementById("chatIcon").onclick = toggleChat;
document.getElementById("closeChat").onclick = toggleChat;

// =========================
// SignalR connection
// =========================
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`/chatHub?userId=${userId}`)
  .build();

connection
  .start()
  .then(() => console.log("Connected to chatHub as user"))
  .catch(err => console.error(err.toString()));

// =========================
// Nhận tin nhắn từ Admin (Realtime)
// =========================
connection.on("ReceiveMessage", (from, message, type) => {
  if (type === "Admin") {
    showMessage(message, "Admin");
  }
});

// =========================
// Load lịch sử chat từ API
// =========================
function loadHistory() {
  fetch(`/Chats/GetHistory?userId=${userId}`)
    .then(res => res.json())
    .then(res => {
      const messages = res.result || res; 

      const chatBody = document.getElementById("chatBody");
      chatBody.innerHTML = "";

      messages.forEach(m => {
        const msg = m.message || m.Message || "";
        const sender = m.sender || m.Sender || "User";
        const type = sender === "Admin" ? "Admin" : "Client";

        showMessage(msg, type);
      });

      chatBody.scrollTop = chatBody.scrollHeight;
    })
    .catch(err => console.error("Load history error:", err));
}


// =========================
// Gửi tin nhắn từ Client -> Admin
// =========================
document.getElementById("sendBtn").onclick = () => {
  const input = document.getElementById("chatInput");
  const message = input.value.trim();

  if (!message) return;

  connection
    .invoke("SendMessageToAdmin", userId, message)
    .catch(err => console.error(err.toString()));

  showMessage(message, "Client");

  input.value = "";
  input.focus();
};

// =========================
// Hiển thị tin nhắn
// =========================
function showMessage(message, type) {
  const chatBody = document.getElementById("chatBody");
  const div = document.createElement("div");

  div.textContent = message;
  div.style.alignSelf = type === "Client" ? "flex-end" : "flex-start";
  div.style.background =
    type === "Client"
      ? "linear-gradient(135deg,#4A90E2,#7B68EE)"
      : "#f1f1f1";
  div.style.color = type === "Client" ? "#fff" : "#333";
  div.style.padding = "10px";
  div.style.borderRadius = "12px";
  div.style.maxWidth = "85%";
  div.style.wordBreak = "break-word";
  div.style.margin = "5px 0";

  chatBody.appendChild(div);
  chatBody.scrollTop = chatBody.scrollHeight;
}

// =========================
// Enter để gửi, Shift+Enter xuống dòng
// =========================
document.getElementById("chatInput").addEventListener("keydown", e => {
  if (e.key === "Enter" && !e.shiftKey) {
    e.preventDefault();
    document.getElementById("sendBtn").click();
  }
});

// =========================
// Mặc định chỉ hiện icon chat
// =========================
document.getElementById("chatIcon").style.display = "flex";
