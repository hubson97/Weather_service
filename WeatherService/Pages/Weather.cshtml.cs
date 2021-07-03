using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using WeatherService.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using WeatherService.Model;
using Microsoft.AspNetCore.SignalR;
using WeatherService.Hubs;

namespace WeatherService.Pages
{
    [Authorize]
    public class WeatherModel : PageModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHubContext<WeatherHub> _hubContext;

        public IList<SelectListItem> DataTypeList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem(){ Text="Dane bie¿¹ce", Value = "currentData" },
            new SelectListItem(){ Text="Wartoœæ œrednia", Value = "avgValue" },
            new SelectListItem(){ Text="Wartoœæ odchylenia standardowego", Value = "stdDeviationValue" }
        };

        public IList<SelectListItem> CityList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem(){ Text="Warszawa", Value = "warszawa" },
            new SelectListItem(){ Text="£ódŸ", Value = "lodz" },
            new SelectListItem(){ Text="Szczecin", Value = "szczecin" },
            new SelectListItem(){ Text="Wroc³aw", Value = "wroclaw" },
            new SelectListItem(){ Text="Gdañsk", Value = "gdansk" },
            new SelectListItem(){ Text="Kraków", Value = "krakow" },
            new SelectListItem(){ Text="Suwa³ki", Value = "suwalki" },
            new SelectListItem(){ Text="Rzeszów", Value = "rzeszow" }
        };

        public IList<SelectListItem> WthSrvList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem(){ Text="OpenWeather", Value = "openweather" },
            new SelectListItem(){ Text="Weatherbit", Value = "weatherbit" },
            new SelectListItem(){ Text="AccuWeather", Value = "accuweather" }
        };

        [BindProperty(SupportsGet = true)]
        public WeatherOptions Options { get; set; } = new WeatherOptions() { DataSrc=null,DataType= null, City= null, DaysNumber=1};

        public int DbDaysNumber { get; set; } = 1;

        public WeatherModel(ApplicationDbContext dbContext, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IHubContext<WeatherHub> hubContext)
        {
            _dbContext = dbContext;
            _signInManager = signInManager;
            _userManager = userManager; 
            _hubContext = hubContext;
        }

        public void OnGet()
        {
            DbDaysNumber = getDaysNumberFromDb();



            var xx = _dbContext.WeatherInfos.Where(x => x.City.Equals("warszawa")).Select(x => x);
            string a = xx.Select(x => x.City).First();
            double b = xx.Select(x => x.Pressure).First();

        }


        public void OnPost()
        {
        }

        public void OnPostWeatherOptions()//OnGetWeatherOptions()
        {
            //var weatherInfo = getNewWeatherInfo();

            //var type = Options.DataType;
            //if (type != null)
            //{
            //    if (type.Equals("currentData"))
            //    {

            //    }
            //    else if (type.Equals("avgValue"))
            //    {

            //    }
            //    else if (type.Equals("stdDeviationValue"))
            //    {

            //    }
            //}


            //var a = Options.DataType;
            //var b = Options.DataSrc;
            //var c = Options.City;
            //var d = Options.DaysNumber;
            //return RedirectToPage("Weather");
        }

        private int getDaysNumberFromDb()
        {
            var date = _dbContext.WeatherInfos.Select(x => x.DateTime).OrderBy(x => x).First();
            return (DateTime.Now - date).Days;
        }

        private WeatherInfoModel getNewWeatherInfo()
        {
            var wthModel = new WeatherInfoModel()
            {
                WeatherServiceName = "OpenWeather",
                City = "Warszawa",
                DateTime = DateTime.Now,
                Temperature = 27.0,
                Pressure = 900.0,
                Humidity = 30.0,
                Rain = 20.0,
                WindSpeed = 33,
                WindDirection = 25
            };

            return wthModel;
        }


    }
}
