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
            new SelectListItem(){ Text="Current data", Value = "currentData" },
            new SelectListItem(){ Text="Average values", Value = "avgValue" },
            new SelectListItem(){ Text="Standard deviation value", Value = "stdDeviationValue" }
        };

        public IList<SelectListItem> CityList { get; set; } = new List<SelectListItem>()
        {
            new SelectListItem(){ Text="Warszawa", Value = "warszawa" },
            new SelectListItem(){ Text="Lodz", Value = "lodz" },
            new SelectListItem(){ Text="Szczecin", Value = "szczecin" },
            new SelectListItem(){ Text="Wroclaw", Value = "wroclaw" },
            new SelectListItem(){ Text="Gdansk", Value = "gdansk" },
            new SelectListItem(){ Text="Krakow", Value = "krakow" },
            new SelectListItem(){ Text="Suwalki", Value = "suwalki" },
            new SelectListItem(){ Text="Rzeszow", Value = "rzeszow" }
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
        }

        public void OnPost()
        {
        }

        private int getDaysNumberFromDb()
        {
            var date = _dbContext.WeatherInfos.Select(x => x.DateTime).OrderBy(x => x).First();
            var diff = (DateTime.Now - date).TotalDays;

            int left = (int)diff;

            var diff2 = diff - left;
            if (diff2 > 0)
                return left + 1;
            else
                return left;
        }

    }
}
