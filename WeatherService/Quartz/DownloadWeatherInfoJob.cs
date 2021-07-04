using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WeatherService.Data;

namespace WeatherService.Quartz
{
    [DisallowConcurrentExecution]
    public class DownloadWeatherInfoJob : IJob
    {
        //private readonly ApplicationDbContext _dbContext;

        private readonly ILogger<DownloadWeatherInfoJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient httpClient = new HttpClient();

        public DownloadWeatherInfoJob(ILogger<DownloadWeatherInfoJob> logger, IServiceProvider serviceProvider ) //ApplicationDbContext dbContext)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }



        public Task Execute(IJobExecutionContext context)
        {

            var wthDataList = GetNewWeatherDataList();
            

            using ( var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

                foreach(var wth in wthDataList)
                {
                    dbContext.Add(wth);
                }
                var ile = dbContext.SaveChanges();

            }

            _logger.LogInformation("New data to database added successfully!");

            return Task.CompletedTask;
        }
        private List<WeatherInfo> GetNewWeatherDataList()
        {
            var dataList = new List<WeatherInfo>();

            var weatherbitDataList = GetWeatherbitDataList();
            if (weatherbitDataList != null)
                dataList.AddRange(weatherbitDataList);


            var accuweatherDataList = GetAccuweatherDataList();
            if (accuweatherDataList != null)
                dataList.AddRange(accuweatherDataList);


            var openweatherDataList = GetOpenweatherDataList();
            if (openweatherDataList != null)
                dataList.AddRange(openweatherDataList);

            return dataList;
        }

        private List<WeatherInfo> GetWeatherbitDataList()
        {
            var dataList = new List<WeatherInfo>();

            foreach (var city in CityCoords)
            {
                try
                {
                    string lati = city.Value.Lati.ToString().Replace(",", ".");
                    string longi = city.Value.Longi.ToString().Replace(",", ".");

                    var urlStr = $"https://api.weatherbit.io/v2.0/current?lat={lati}&lon={longi}&key=3af413420f5f45e990facb128d9ca983&lang=pl";
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = httpClient.GetAsync(urlStr).Result.EnsureSuccessStatusCode();

                    string jsonString = res.Content.ReadAsStringAsync().Result;
                    JObject jsonObj = JObject.Parse(jsonString);
                    if (jsonObj == null)
                        continue;

                    var wth = new WeatherInfo()
                    {
                        WeatherServiceName = "weatherbit",
                        City = city.Key,
                        DateTime = DateTime.Now,
                        Temperature =   Math.Round(Convert.ToDouble(jsonObj["data"][0]["temp"]),2),
                        Pressure =      Math.Round(Convert.ToDouble(jsonObj["data"][0]["pres"]),2),
                        Humidity =      Math.Round(Convert.ToDouble(jsonObj["data"][0]["rh"]),2),
                        WindSpeed =     Math.Round(Convert.ToDouble(jsonObj["data"][0]["wind_spd"]),2),
                        WindDirection = Math.Round(Convert.ToDouble(jsonObj["data"][0]["wind_dir"]),2),
                        Rain =          Math.Round(Convert.ToDouble(jsonObj["data"][0]["precip"]), 2),
                    };

                    dataList.Add(wth);

                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            if (dataList != null && dataList.Count == CityCoords.Count)
            {
                var wthPol = new WeatherInfo()
                {
                    WeatherServiceName = "weatherbit",
                    City = "poland",
                    DateTime = DateTime.Now,
                    Temperature = Math.Round(dataList.Average(p => p.Temperature), 2),
                    Pressure    = Math.Round(dataList.Average(p => p.Pressure), 2),
                    Humidity    = Math.Round(dataList.Average(p => p.Humidity), 2),
                    Rain        = Math.Round(dataList.Average(p => p.Rain), 2),
                    WindSpeed   = Math.Round(dataList.Average(p => p.WindSpeed), 2),
                    WindDirection = Math.Round(dataList.Average(p => p.WindDirection), 2)
                };
                dataList.Add(wthPol);
            }

            return dataList;
        }

        private List<WeatherInfo> GetAccuweatherDataList()
        {
            var dataList = new List<WeatherInfo>();

            foreach (var city in CityCoords)
            {
                try
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var locationUrlStr = $"http://dataservice.accuweather.com/locations/v1/cities/geoposition/search?apikey=viA7ViOeIutUmJEilVyqlqJcSOB4XN29&q={city.Value.Lati}%2C{city.Value.Longi}&language=pl&details=false&toplevel=false";
                    HttpResponseMessage res = httpClient.GetAsync(locationUrlStr).Result.EnsureSuccessStatusCode();
                    var locKeyStr = JObject.Parse(res.Content.ReadAsStringAsync().Result)["Key"].ToString();

                    var weatherUrlStr = $"http://dataservice.accuweather.com/currentconditions/v1/{locKeyStr}?apikey=viA7ViOeIutUmJEilVyqlqJcSOB4XN29&language=pl&details=true";

                    HttpResponseMessage res2 = httpClient.GetAsync(weatherUrlStr).Result.EnsureSuccessStatusCode();

                    string jsonString = res2.Content.ReadAsStringAsync().Result;
                    //wycinanie pierwszego i ostatniego znaku tablicowego [] w jsonie, przez ktorego wywalalo exception
                    jsonString = jsonString.Substring(1);
                    jsonString = jsonString.Substring(0, jsonString.Length-1);

                    JObject jsonObj = JObject.Parse(jsonString);
                    if (jsonObj == null)
                        continue;

                    var wth = new WeatherInfo()
                    {
                        WeatherServiceName = "accuweather",
                        City = city.Key,
                        DateTime = DateTime.Now,
                        Temperature = Math.Round(Convert.ToDouble(jsonObj["Temperature"]["Metric"]["Value"]), 2),
                        Pressure = Math.Round(Convert.ToDouble(jsonObj["Pressure"]["Metric"]["Value"]), 2),
                        Humidity = Math.Round(Convert.ToDouble(jsonObj["RelativeHumidity"]), 2),
                        WindSpeed = Math.Round(Convert.ToDouble(jsonObj["Wind"]["Speed"]["Metric"]["Value"]), 2),
                        WindDirection = Math.Round(Convert.ToDouble(jsonObj["Wind"]["Direction"]["Degrees"]), 2),
                        Rain = Math.Round(Convert.ToDouble(jsonObj["PrecipitationSummary"]["PastHour"]["Metric"]["Value"]) ,2)
                    };

                    dataList.Add(wth);

                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            if (dataList != null && dataList.Count == CityCoords.Count)//== CityCoords.Count)
            {
                var wthPol = new WeatherInfo()
                {
                    WeatherServiceName = "accuweather",
                    City = "poland",
                    DateTime = DateTime.Now,
                    Temperature = Math.Round(dataList.Average(p => p.Temperature), 2),
                    Pressure    = Math.Round(dataList.Average(p => p.Pressure),2),
                    Humidity    = Math.Round(dataList.Average(p => p.Humidity),2),
                    Rain        = Math.Round(dataList.Average(p => p.Rain),2),
                    WindSpeed   = Math.Round(dataList.Average(p => p.WindSpeed),2),
                    WindDirection = Math.Round(dataList.Average(p => p.WindDirection),2)
                };
                dataList.Add(wthPol);
            }

            return dataList;
        }

        private List<WeatherInfo> GetOpenweatherDataList()
        {
            var dataList = new List<WeatherInfo>();

            foreach(var city in CityCoords)
            {
                try
                {
                    var urlStr = $"https://api.openweathermap.org/data/2.5/onecall?lat={city.Value.Lati}&lon={city.Value.Longi}&appid=2466076106194b4f2be01cf4d7edbe51&exclude=minutely,hourly&lang=pl";
                    httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage res = httpClient.GetAsync(urlStr).Result.EnsureSuccessStatusCode();

                    string jsonString = res.Content.ReadAsStringAsync().Result;
                    JObject jsonObj = JObject.Parse(jsonString);
                    if (jsonObj == null)
                        continue;

                    var wth = new WeatherInfo()
                    {
                        WeatherServiceName = "openweather",
                        City = city.Key,
                        DateTime = DateTime.Now,
                        Temperature = Math.Round(getCelsiusFromKelvin(Convert.ToDouble(jsonObj["current"]["temp"])), 2),
                        Pressure = Math.Round(Convert.ToDouble(jsonObj["current"]["pressure"]), 2),
                        Humidity = Math.Round(Convert.ToDouble(jsonObj["current"]["humidity"]), 2),
                        WindSpeed = Math.Round(Convert.ToDouble(jsonObj["current"]["wind_speed"]), 2),
                        WindDirection = Math.Round(Convert.ToDouble(jsonObj["current"]["wind_deg"]), 2),
                        Rain = Math.Round(Convert.ToDouble(jsonObj["daily"][0]["rain"]), 2)
                    };

                    dataList.Add(wth);

                }
                catch(HttpRequestException ex)
                {
                    _logger.LogError(ex.Message);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            if(dataList != null && dataList.Count == CityCoords.Count)//== CityCoords.Count)
            {
                var wthPol = new WeatherInfo()
                {
                    WeatherServiceName = "openweather",
                    City = "poland",
                    DateTime = DateTime.Now,
                    Temperature = Math.Round(dataList.Average(p => p.Temperature), 2),
                    Pressure = Math.Round(dataList.Average(p => p.Pressure), 2),
                    Humidity = Math.Round(dataList.Average(p => p.Humidity), 2),
                    Rain = Math.Round(dataList.Average(p => p.Rain), 2),
                    WindSpeed = Math.Round(dataList.Average(p => p.WindSpeed), 2),
                    WindDirection = Math.Round(dataList.Average(p => p.WindDirection), 2)
                };
                dataList.Add(wthPol);
            }

            return dataList;
        }

        public double getCelsiusFromKelvin(double val) => val - 273.15;

        public Dictionary<string, Coords> CityCoords { get; set; } = new Dictionary<string, Coords>()
        {
            { "warszawa",new Coords{ Lati=52.24,Longi=21.01} },
            { "lodz",new Coords{ Lati=51.76,Longi=19.46} },
            { "wroclaw",new Coords{ Lati=51.11,Longi=17.04} },
            { "szczecin",new Coords{ Lati=53.43,Longi=14.55} },
            { "rzeszow",new Coords{ Lati=50.04,Longi=22.0} },
            { "krakow",new Coords{ Lati=50.05,Longi=19.94} },
            { "gdansk",new Coords{ Lati=54.37,Longi=18.64} },
            { "suwalki",new Coords{ Lati=54.10,Longi=22.93} },
        };

        


        //var wth = new WeatherInfo()
        //{
        //    WeatherServiceName = "testowy",
        //    City = "testowe",
        //    DateTime = DateTime.Now,
        //    Temperature = -15,
        //    Pressure = 1000,
        //    Humidity = 5.0,
        //    Rain = 5.0,
        //    WindSpeed = 5,
        //    WindDirection = 50
        //};


    }

    public class Coords
    {
        public double Lati { get; set; }
        public double Longi { get; set; }
    }

}
