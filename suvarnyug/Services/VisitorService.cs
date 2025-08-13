using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Suvarnyug.Data;
using Suvarnyug.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Suvarnyug.Services
{
    public class VisitorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VisitorService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogVisitorAsync()
        {
            var context = _httpContextAccessor.HttpContext;

            // Get IP from proxy headers first
            string ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                               ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
                               ?? context.Connection.RemoteIpAddress?.ToString();

            // If multiple IPs in X-Forwarded-For, take the first one
            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(","))
            {
                ipAddress = ipAddress.Split(',').First().Trim();
            }

            // If still empty, stop
            if (string.IsNullOrEmpty(ipAddress))
                return;

            // Check if this IP has visited in the last 6 hours
            var lastVisit = _context.DailyVisitors
                .Where(v => v.IpAddress == ipAddress)
                .OrderByDescending(v => v.VisitDateTime)
                .FirstOrDefault();

            if (lastVisit != null && lastVisit.VisitDateTime > DateTime.UtcNow.AddHours(-6))
            {
                // Skip logging because user already visited within last 6 hours
                return;
            }

            // Get country from IP API
            string country = "Unknown";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var json = await httpClient.GetStringAsync($"https://ipapi.co/{ipAddress}/json/");
                    var data = JObject.Parse(json);
                    country = data["country_name"]?.ToString() ?? "Unknown";
                }
            }
            catch
            {
                // Keep "Unknown" if API fails
            }

            // Save visitor log
            var visitor = new DailyVisitor
            {
                IpAddress = ipAddress,
                Country = country,
                VisitDateTime = DateTime.UtcNow
            };

            _context.DailyVisitors.Add(visitor);
            await _context.SaveChangesAsync();
        }
    }
}