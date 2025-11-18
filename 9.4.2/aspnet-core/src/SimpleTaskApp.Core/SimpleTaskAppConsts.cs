using SimpleTaskApp.Debugging;

namespace SimpleTaskApp
{
    public class SimpleTaskAppConsts
    {
        public const string LocalizationSourceName = "SimpleTaskApp";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "6bbd8f6d02604397917079e78ab904d2";
    }
}
