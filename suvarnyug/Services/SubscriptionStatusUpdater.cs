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

                    var expiredSubscriptions = dbContext.Subscriptions
                        .Where(s => s.EndDate < DateTime.Now && s.IsActive)
                        .ToList();

                    foreach (var subscription in expiredSubscriptions)
                    {
                        subscription.IsActive = false;
                        subscription.PaymentStatus = "Pending";
                        subscription.PlanType = PlanType.Free;
                    }

                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}
