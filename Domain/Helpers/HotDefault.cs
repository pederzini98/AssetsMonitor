using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helpers
{
    public class HotDefault
    {
        private static readonly string _sellSubject = "ALERT about {x} value! Time to sell it! ";
        public static string SellSubject { get => _sellSubject; }

        private static readonly string _buySubject = "ALERT about {x} value! Time to buy it!";
        public static string BuySubject { get => _buySubject; }


        private static readonly string _senderName = "Free Asserts News";
        public static string SenderName { get => _senderName; }
    }
}
