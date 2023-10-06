using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Projecttask.Data;
using Projecttask.Models;
using Projecttask.Services.Handlers;
using Projecttask.Services.Interfaces;
using Serilog.Events;
using Serilog;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var outputTemplate = "\"[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine} Environment:{Environment} ThreadId: {ThreadId} {Exception}\"";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/myapp.txt")
    .WriteTo.MSSqlServer(builder.Configuration.GetConnectionString("default"), "logs", autoCreateSqlTable: true)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("default")).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).EnableSensitiveDataLogging(), ServiceLifetime.Transient);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = true;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(op => op.TokenLifespan = TimeSpan.FromHours(10));

var mailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<MailConfiguration>();
builder.Services.AddSingleton(mailConfig);
builder.Services.AddScoped<IMailService, MailService>();

var serviceProvider = builder.Services.BuildServiceProvider();
var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

var roles = new List<string> { "Admin", "Employer", "Worker" };
foreach (var roleName in roles)
{
    if (!roleManager.RoleExistsAsync(roleName).Result)
    {
        var role = new IdentityRole { Name = roleName };
        var result = roleManager.CreateAsync(role).Result;
        if (!result.Succeeded)
        {
        }
    }
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/Login";
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddControllersWithViews();
builder.Services.AddCoreAdmin("Admin");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        await context.Response.WriteAsync("Request Access Denied");
    }
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Employer}/{action=Index}");

});

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.MapRazorPages();
app.MapDefaultControllerRoute();
app.UseCoreAdminCustomTitle("Resul Site Admin Panel");
app.UseCoreAdminCustomUrl("admin");

app.Run();
