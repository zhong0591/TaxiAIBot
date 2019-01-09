using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxiAIBot.Model
{
    [Serializable]
    public class GPS
    {
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string Formatted { get; set; }
    }
}
