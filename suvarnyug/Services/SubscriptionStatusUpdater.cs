using suvarnyug.Models;
using Suvarnyug.Data;

namespace suvarnyug.Services
{
    public class SubscriptionStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SubscriptionStatusUpdater(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var now = DateTime.Now;


                    var expiredSubscriptions = dbContext.Subscriptions
                        .Where(s => s.EndDate < now && s.IsActive)
                        .ToList();

                    foreach (var subscription in expiredSubscriptions)
                    {
                        subscription.IsActive = false;
                        subscription.PaymentStatus = "Pending";
                        subscription.PlanType = PlanType.Free;

                        var maleBiodatas = dbContext.Biodata
                            .Where(b => b.UserId == subscription.UserId && b.Gender.ToLower() == "male").ToList();

                        foreach (var biodata in maleBiodatas)
                        {
                            biodata.IsPremiumActive = true; // hide male biodata
                        }
                    }
                    var activeSubscriptions = dbContext.Subscriptions
                        .Where(s => s.EndDate >= now && s.IsActive)
                        .ToList();

                    foreach (var subscription in activeSubscriptions)
                    {
                        // Set male biodata IsPremiumActive = 0
                        var maleBiodatas = dbContext.Biodata
                            .Where(b => b.UserId == subscription.UserId && b.Gender.ToLower() == "male" && b.User.Role != "Admin")
                            .ToList();

                        foreach (var biodata in maleBiodatas)
                        {
                            biodata.IsPremiumActive = false; // make male biodata visible
                        }
                    }


                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }
}
