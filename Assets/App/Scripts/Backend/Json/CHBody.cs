using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    namespace Request
    {
        public class ButtonBet
        {
            public string button { get; set; }
            public string type { get; set; }
            public double amount { get; set; }
        }

        public class Root
        {
            public double total_amount { get; set; }
            public List<ButtonBet> button_bet { get; set; }
        }
    }
}
