using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherService.Model
{
    public class WeatherInfoModel
    {
        [JsonProperty("weatherServiceName")]
        public string WeatherServiceName { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("dateTime")]
        public DateTime DateTime { get; set; }
        [JsonProperty("temperature")]
        public double Temperature { get; set; }
        [JsonProperty("pressure")]
        public double Pressure { get; set; }
        [JsonProperty("humidity")]
        public double Humidity { get; set; }
        [JsonProperty("rain")]
        public double Rain { get; set; }
        [JsonProperty("windSpeed")]
        public double WindSpeed { get; set; }
        [JsonProperty("windDirection")]
        public double WindDirection { get; set; }
    }
}
