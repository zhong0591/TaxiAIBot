using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxiAIBot.Model
{
    [Serializable]
    public class Payment
    {
        public float Cost { get; set; }
        public float Price { get; set; }
        public float Fixed { get; set; } = 0;
    }
}
