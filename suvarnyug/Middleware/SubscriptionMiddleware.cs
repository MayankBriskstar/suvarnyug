using Suvarnyug.Data;
using System.Security.Claims;

namespace suvarnyug.Middleware
{
    public class SubscriptionMiddleware
    {
        private readonly RequestDelegate _next;

        public SubscriptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null)
            {
                var userId = int.Parse(userIdClaim.Value);
                var subscription = dbContext.Subscriptions.FirstOrDefault(s => s.UserId == userId && s.IsActive && s.PaymentStatus == "Pending");

                if (subscription != null)
                {
                    context.Response.Redirect("/Payment/Subscribe");
                    return;
                }
            }

            await _next(context);
        }
    }

}
