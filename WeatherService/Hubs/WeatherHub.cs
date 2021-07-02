using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeatherService.Model;
using WeatherService.Data;

namespace WeatherService.Hubs
{
    public class WeatherHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;

        //[JsonProperty("dbDaysNumber")]
        private int DbDaysNumber { get; set; } = 1;

        public WeatherOptions WthOptions { get; set; }

        public WeatherHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendNewWeatherData(WeatherOptions options )
        {
            //1. Client te wywoluje funkcje w celu pobrania danych, ktore go interesują - okreslone przekazanymi parametrami
            WthOptions = options;


            //2. Pobranie liczby dni z jakich byla pobierana pogoda:
            DbDaysNumber = getDaysNumberFromDb();


            //3. Server pobiera odpowienie dane z bazy danych w zaleznosci od oczekiwan
            //getInfoFromDatabase(); // lista weatherInfoModel
            var wthDataList = getWeatherInfoFromDb();


            //5. Serwer wysyła odpowiednie dane do clienta w zależności od oczekiwań

            if (options.DataType.Equals("currentData")) 
            { 
                await Clients.Caller.SendAsync("newCurrDataWeather", wthDataList, DbDaysNumber);
            }
            else if (options.DataType.Equals("avgValue"))
            {
                await Clients.Caller.SendAsync("newAvgDataWeather", wthDataList, DbDaysNumber);
            }
            else if (options.DataType.Equals("stdDeviationValue"))
            { 
                await Clients.Caller.SendAsync("newStdDeviationDataWeather", wthDataList, DbDaysNumber);
            }

        }

        private int getDaysNumberFromDb()
        {
            var date = _dbContext.WeatherInfos.Select(x => x.DateTime).OrderBy(x=>x).First();
            return ( DateTime.Now - date).Days;
        }

        public List<WeatherInfoModel> getWeatherInfoFromDb()
        {
            var dbDataList = _dbContext.WeatherInfos.ToList();
            DbDaysNumber = WthOptions.DaysNumber > DbDaysNumber ? DbDaysNumber : WthOptions.DaysNumber;

            var wthDataList = new List<WeatherInfoModel>();
            //int totalDbDays = getDaysNumberFromDb();

            if (WthOptions.DataType.Equals("currentData"))
            {
                var filteredDataList = extractCitiesCurrentWeatherInfo(dbDataList);

                if (filteredDataList.Count.Equals(0))
                {
                    wthDataList = null;
                }
                else
                    wthDataList = filteredDataList;

                //var polandData = extractPolandCurrentWeatherInfo(dbDataList);
                //if (polandData != null && wthDataList != null)
                //    wthDataList.Add(polandData);
                //else if (polandData != null && wthDataList == null)
                //    wthDataList = new List<WeatherInfoModel>() { polandData };

            }
            else if (WthOptions.DataType.Equals("avgValue"))
            {
                

                var filteredDataList = extractCitiesAverageWeatherInfo(dbDataList);

                if (filteredDataList.Count.Equals(0))
                {
                    wthDataList = null;
                }
                else
                    wthDataList = filteredDataList;

                //var polandData = extractPolandAverageWeatherInfo(dbDataList);
                //if (polandData != null && wthDataList != null)
                //    wthDataList.Add(polandData);
                //else if (polandData != null && wthDataList == null)
                //    wthDataList = new List<WeatherInfoModel>() { polandData };

            }
            else if (WthOptions.DataType.Equals("stdDeviationValue"))
            {

                var filteredDataList = extractWthStationsStdDeviationWeatherInfo( dbDataList );

                if (filteredDataList.Count.Equals(0))
                {
                    wthDataList = null;
                }
                else
                    wthDataList = filteredDataList;

                var allDataModel = extractAllStationsStdDeviationWeatherInfo(dbDataList);
                if (allDataModel != null && wthDataList != null)
                    wthDataList.Add(allDataModel);
                else if (allDataModel != null && wthDataList == null)
                    wthDataList = new List<WeatherInfoModel>() { allDataModel };
            }

            return wthDataList;
        }


        private List<WeatherInfoModel> extractCitiesCurrentWeatherInfo(List<WeatherInfo> dbDataList)
        {

            var filteredDataList = dbDataList.Where(x => x.WeatherServiceName.Equals(WthOptions.DataSrc))
                                    .GroupBy(x => x.City, (key, g) => g.OrderByDescending(e => e.DateTime)
                                      .Select(x => new WeatherInfoModel()
                                      {
                                          WeatherServiceName = x.WeatherServiceName,
                                          City = x.City,
                                          DateTime = x.DateTime,
                                          Temperature = x.Temperature,
                                          Pressure = x.Pressure,
                                          Humidity = x.Humidity,
                                          Rain = x.Rain,
                                          WindSpeed = x.WindSpeed,
                                          WindDirection = x.WindDirection
                                      })
                                      .FirstOrDefault()).ToList();
            return filteredDataList;
        }
        private WeatherInfoModel extractPolandCurrentWeatherInfo(List<WeatherInfo> dbDataList)
        {

            var filteredData = dbDataList.Where(x => x.WeatherServiceName.Equals(WthOptions.DataSrc) && x.City.Equals("poland"))
                                      .OrderByDescending(d=>d.DateTime)
                                      .Select(x => new WeatherInfoModel()
                                      {
                                          WeatherServiceName = x.WeatherServiceName,
                                          City = x.City,
                                          DateTime = x.DateTime,
                                          Temperature = x.Temperature,
                                          Pressure = x.Pressure,
                                          Humidity = x.Humidity,
                                          Rain = x.Rain,
                                          WindSpeed = x.WindSpeed,
                                          WindDirection = x.WindDirection
                                      })
                                      .FirstOrDefault();
            return filteredData;
        }



        private List<WeatherInfoModel>  extractCitiesAverageWeatherInfo(List<WeatherInfo> dbDataList)
        {
            var filteredDataList = dbDataList
                .Where(x => x.WeatherServiceName.Equals(WthOptions.DataSrc) && (DateTime.Now - x.DateTime).Days <= DbDaysNumber)
                .GroupBy(x => x.City, (key, g) =>
                g.Select(x => new WeatherInfoModel()
                {

                    WeatherServiceName = x.WeatherServiceName,
                    City = x.City,
                    DateTime = x.DateTime,
                    Temperature = Math.Round(g.Average(p => p.Temperature), 2),//x.Temperature,
                    Pressure = Math.Round(g.Average(p => p.Pressure), 2),//x.Pressure,
                    Humidity = Math.Round(g.Average(p => p.Humidity), 2),//x.Humidity,
                    Rain = Math.Round(g.Average(p => p.Rain), 2), //x.Rain,
                    WindSpeed = Math.Round(g.Average(p => p.WindSpeed), 2),//x.WindSpeed,
                    WindDirection = Math.Round(g.Average(p => p.WindDirection), 2) //x.WindDirection
                })
                .FirstOrDefault()).ToList();

            return filteredDataList;
        }
        private WeatherInfoModel extractPolandAverageWeatherInfo(List<WeatherInfo> dbDataList)
        {
            var data = dbDataList.Where(x => x.WeatherServiceName.Equals(WthOptions.DataSrc) 
                                        && x.City.Equals("poland")
                                        && (DateTime.Now - x.DateTime).Days <= DbDaysNumber
                                        ).ToList();
            var polandWthModel = new WeatherInfoModel()
            {
                WeatherServiceName = data[0].WeatherServiceName,
                City = "Poland", //data[0].City,
                DateTime = data[0].DateTime,
                Temperature = Math.Round(data.Average(p => p.Temperature), 2),
                Pressure = Math.Round(data.Average(p => p.Pressure), 2),
                Humidity = Math.Round(data.Average(p => p.Humidity), 2),
                Rain = Math.Round(data.Average(p => p.Rain), 2),
                WindSpeed = Math.Round(data.Average(p => p.WindSpeed), 2),
                WindDirection = Math.Round(data.Average(p => p.WindDirection), 2)
            };

            return polandWthModel;
        }


        private List<WeatherInfoModel> extractWthStationsStdDeviationWeatherInfo(List<WeatherInfo> dbDataList)
        {
            var filteredDataList = dbDataList.Where(x => x.City.Equals(WthOptions.City) && (DateTime.Now - x.DateTime).Days <= DbDaysNumber)
                      .GroupBy(x => x.WeatherServiceName, (key, g) =>
                      g.Select(x => new WeatherInfoModel()
                      {
                          WeatherServiceName = x.WeatherServiceName,
                          City = x.City,
                          DateTime = DateTime.Now,//x.DateTime,
                              Temperature = StdDev(g.Select(p => p.Temperature)),//x.Temperature,
                              Pressure = StdDev(g.Select(p => p.Pressure)),//x.Pressure,
                              Humidity = StdDev(g.Select(p => p.Humidity)),//x.Humidity,
                              Rain = StdDev(g.Select(p => p.Rain)), //x.Rain,
                              WindSpeed = StdDev(g.Select(p => p.WindSpeed)),//x.WindSpeed,
                              WindDirection = StdDev(g.Select(p => p.WindDirection)) //x.WindDirection
                          })
                      .FirstOrDefault()).ToList();

            return filteredDataList;
        }
        private WeatherInfoModel extractAllStationsStdDeviationWeatherInfo(List<WeatherInfo> dbDataList)
        {
            var data = dbDataList.Where(x => x.City.Equals(WthOptions.City) && (DateTime.Now - x.DateTime).Days <= DbDaysNumber).ToList();

            if (data.Count.Equals(0))
                return null;

            var polandWthModel = new WeatherInfoModel()
            {
                WeatherServiceName = data[0].WeatherServiceName,
                City = data[0].City,
                DateTime = data[0].DateTime,
                Temperature = StdDev(data.Select(p => p.Temperature)),//x.Temperature,
                Pressure = StdDev(data.Select(p => p.Pressure)),//x.Pressure,
                Humidity = StdDev(data.Select(p => p.Humidity)),//x.Humidity,
                Rain = StdDev(data.Select(p => p.Rain)), //x.Rain,
                WindSpeed = StdDev(data.Select(p => p.WindSpeed)),//x.WindSpeed,
                WindDirection = StdDev(data.Select(p => p.WindDirection)) //x.WindDirection
            };



            return polandWthModel;
        }



        private double StdDev(IEnumerable<double> values)
        {
            double mean = 0.0;
            double sum = 0.0;
            double stdDev = 0.0;
            int n = 0;
            foreach (double val in values)
            {
                n++;
                double delta = val - mean;
                mean += delta / n;
                sum += delta * (val - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum / (n));

            return Math.Round(stdDev,4);
        }














        //private double CalculateStdDev(IEnumerable<double> values)
        //{
        //    double ret = 0;

        //    if (values.Count() > 0)
        //    {
        //        //Compute the Average
        //        double avg = values.Average();

        //        //Perform the Sum of (value-avg)^2
        //        double sum = values.Sum(d => Math.Pow(d - avg, 2));

        //        //Put it all together
        //        ret = Math.Sqrt((sum) / values.Count() - 1);
        //    }
        //    return ret;
        //}

        //WeatherInfoModel currWeather = new WeatherInfoModel()
        //{
        //    WeatherServiceName = "openweather",
        //    City = "Warszawa",
        //    DateTime = DateTime.Now,
        //    Temperature = 22.0,
        //    Pressure = 900.0,
        //    Humidity = 30.0,
        //    Rain = 20.0,
        //    WindSpeed = 33,
        //    WindDirection = 12
        //};

        //WeatherInfoModel avgWeather = new WeatherInfoModel()
        //{
        //    WeatherServiceName = "meteomatics",
        //    City = "Łódź",
        //    DateTime = DateTime.Now,
        //    Temperature = 24.0,
        //    Pressure = 950.0,
        //    Humidity = 35.0,
        //    Rain = 25.0,
        //    WindSpeed = 37,
        //    WindDirection = 18
        //};

        //WeatherInfoModel stdDevWeather = new WeatherInfoModel()
        //{
        //    WeatherServiceName = "accuweather",
        //    City = "Gdańsk",
        //    DateTime = DateTime.Now,
        //    Temperature = 27.0,
        //    Pressure = 1000.0,
        //    Humidity = 40.0,
        //    Rain = 28.0,
        //    WindSpeed = 39,
        //    WindDirection = 46
        //};



    }
}
