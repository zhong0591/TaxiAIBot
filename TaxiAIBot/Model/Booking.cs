
using Newtonsoft.Json;
using System;

namespace TaxiAIBot.Model
{

    public class Booking
    {
        public string source { get; set; }
        public int eta { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string flight_number { get; set; }
        public Payment payment { get; set; }
        public int account_id { get; set; }
        public Address address { get; set; }
        public Destination destination { get; set; }
        public string vehicle_type { get; set; }
        public string vehicle_group { get; set; }
        public string instructions { get; set; }
        public int driver_id { get; set; }
        public int vehicle_id { get; set; }
        public string vehicle_ref { get; set; }
        public int site_id { get; set; }
        public Via[] vias { get; set; }
        public string return_date { get; set; }
        public string status { get; set; }
        public string extras { get; set; }
        public string language { get; set; }
    }

    public class Payment
    {
        public float tip { get; set; }
        public float tolls { get; set; }
        public int extras { get; set; }
        public float cost { get; set; }
        public float price { get; set; }
        [JsonProperty("fixed")]
        public float payment_fixed { get; set; }
        public float service_charge { get; set; }
        public int discount_price { get; set; }
        public string discount_price_type { get; set; }
        public float total { get; set; }
        public int discount_cost { get; set; }
        public string discount_cost_type { get; set; }
        public int waiting { get; set; }
        public int waiting_cost { get; set; }
        public int waiting_price { get; set; }
        public string voucher { get; set; }
        public string voucherify_code { get; set; }
        public string order_id { get; set; }
        public Cardpayments cardpayments { get; set; }
        public int card_id { get; set; }
        public string status { get; set; }
        public int fixedfare_id { get; set; }
        public Fixedfare fixedfare { get; set; }
        public int tariff_id { get; set; }
        public int passengers { get; set; }
    }

    public class Cardpayments
    {
        public string amount { get; set; }
        public string authcode { get; set; }
        public DateTime payment_date { get; set; }
    }

    public class Fixedfare
    {
        public int id { get; set; }
        public string fixed_distance { get; set; }
        public string fixed_duration { get; set; }
    }

    public class Address
    {
        public string lat { get; set; }
        public string lng { get; set; }
        public string formatted { get; set; }
    }

    public class Destination
    {
        public string lat { get; set; }
        public string lng { get; set; }
        public string formatted { get; set; }
    }

    public class Via
    {
        public string lat { get; set; }
        public string lng { get; set; }
        public string formatted { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
}
