using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherService.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        DbSet<WeatherInfo> WeatherInfos { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }

    public class WeatherInfo
    {
        public int Id { get; set; }
        public string WeatherServiceName { get; set; }
        public string City { get; set; }
        public DateTime DateTime { get; set; }
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public double Humidity { get; set; }
        public double Rain { get; set; }
        public int WindSpeed { get; set; }
        public int WindDirection { get; set; }
    }
}
