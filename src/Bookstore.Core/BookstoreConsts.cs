using Bookstore.Debugging;

namespace Bookstore
{
    public class BookstoreConsts
    {
        public const string LocalizationSourceName = "Bookstore";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "d708bf31db4348f1b0e66a614c45bab6";
    }
}
