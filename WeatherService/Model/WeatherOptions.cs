using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherService.Model
{
    public class WeatherOptions
    {
        [JsonProperty("dataType")]
        public string DataType { get; set; }

        [JsonProperty("dataSrc")]
        public string DataSrc { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("daysNumber")]
        public int DaysNumber { get; set; }

    }
}
