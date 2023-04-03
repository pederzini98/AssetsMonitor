namespace Domain.Helpers
{
    public class HotDefault

    {
        #region Email default

        private static readonly string _sellSubject = "ALERT about {x} value! Time to sell it! ";
        public static string SellSubject { get => _sellSubject; }

        private static readonly string _buySubject = "ALERT about {x} value! Time to buy it!";
        public static string BuySubject { get => _buySubject; }

        private static readonly string _senderName = "Free Asserts News";
        public static string SenderName { get => _senderName; }
        #endregion

        #region Control default

        private static readonly int _sendEmailTimeout = 2;
        public static int SendEmailTimeout { get => _sendEmailTimeout; }

        private static readonly int _apiRequestTimeout = 1;
        public static int ApiRequestTimeout { get => _apiRequestTimeout; }

        #endregion

        #region Requests

        private static readonly string _apiURl = $"https://brapi.dev/api/quote/";
        public static string ApiURl { get => _apiURl; }

        private static readonly string _supportLink = "https://www.linkedin.com/in/lucas-p-5a1b44b9/";
        public static string SupportLink { get => _supportLink; }

        #endregion
    }
}
