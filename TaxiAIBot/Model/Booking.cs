using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaxiAIBot.Model
{
    public class Booking
    {

        public DateTime BookTime { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public GPS Address { get; set; }
        public GPS Destination { get; set; }
        public Payment Payment { get; set; }
    }
}
