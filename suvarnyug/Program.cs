using AspNetCore.ReCaptcha;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using suvarnyug.Middleware;
using suvarnyug.Services;
using Suvarnyug.Data;
using Suvarnyug.Models;
//using suvarnyug.Hubs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
//        new MySqlServerVersion(new Version(8, 0, 25))));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 25)),
        mySqlOptions => 
        {
            mySqlOptions.EnableRetryOnFailure();
            mySqlOptions.CommandTimeout(30);
            mySqlOptions.MaxBatchSize(100);
            mySqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        })
        .EnableDetailedErrors(builder.Environment.IsDevelopment())
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()));


builder.Services.AddReCaptcha(options =>
{
    options.SiteKey = builder.Configuration["ReCaptcha:SiteKey"];
    options.SecretKey = builder.Configuration["ReCaptcha:SecretKey"];
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".Suvarnyug.Session";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});
builder.Services.AddTransient<IEmailService>(provider =>
    new EmailService(
        builder.Configuration["Email:FromAddress"],
        builder.Configuration["Email:FromPassword"],
        builder.Configuration["Email:SmtpServer"],
        int.Parse(builder.Configuration["Email:SmtpPort"])
    ));
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<SubscriptionStatusUpdater>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("https://192.168.1.79")  // Replace with your actual domain
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials()
               .SetIsOriginAllowed(origin => true)  // For development only
               .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<SubscriptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    await next.Invoke();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/Shared/NotFound";
        await next.Invoke();
    }
});

app.MapHub<NotificationHub>("/notificationHub");
//app.MapHub<ChatHub>("/chathub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();