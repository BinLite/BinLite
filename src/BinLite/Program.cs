using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace BinLite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            SiteDbContext.ConnectionString = builder.Configuration["SQL:BaseConnectionString"];

            builder.Services.AddDbContext<SiteDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Discord";
            })

            .AddCookie(options =>
            {
                options.LoginPath = "/signin";
                options.LogoutPath = "/signout";
            })

            .AddDiscord(options =>
            {
                options.ClientId = builder.Configuration["discord:clientid"];
                options.ClientSecret = builder.Configuration["discord:clientsecret"];
                options.Scope.Add("identify");
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}