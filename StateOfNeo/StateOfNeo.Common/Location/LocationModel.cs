using Newtonsoft.Json;

namespace StateOfNeo.Common
{
    public class LocationModel
    {
        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }
        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }
        [JsonProperty(PropertyName = "country_name")]
        public string CountryName { get; set; }
        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }
        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }
        [JsonProperty(PropertyName = "flag")]
        public string Flag { get; set; }
    }
}
