﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace BinLiteServer
{
    public static class APIManager
    {
        public static void Init()
        {
            if (!Directory.Exists("www")) { Directory.CreateDirectory("www"); }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                ContentRootPath = Directory.GetCurrentDirectory(),
                WebRootPath = "www",
            });

            builder.Services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
                ("BasicAuthentication", null);
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = new PathString("/login.html");
                    options.Cookie.Name = "BinLiteLogin";
                    options.LoginPath = new PathString("/login.html");
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();

            var https = Configuration.Get<JArray>("api.urls").Any(u => u.Value<string>()!.StartsWith("https"));
            Logger.Info("Mode: " + (https ? "Secured HTTPS" : "Insecure HTTP"));

            if (https)
            {
                var certPath = Configuration.Get<string>("api.pfx");
                var certPass = Configuration.Get<string>("api.pfx-pw");
                if (!File.Exists(certPath))
                {
                    throw new FileNotFoundException("Can't find provided api.pfx path.");
                }

                builder.WebHost.ConfigureKestrel((options) =>
                {
                    options.ConfigureHttpsDefaults(listenOptions =>
                    {
                        try
                        {
                            listenOptions.ServerCertificate = new X509Certificate2(certPath, certPass);

                        }
                        catch (Exception ex)
                        {
                            Logger.Fatal("Incorrect PFX password!\n" + ex);
                            return;
                        }
                    });
                });
            }

            var app = builder.Build();
            app.UseFileServer();
            app.UseAuthentication();
            app.UseAuthorization();

            foreach (var url in Configuration.Get<JArray>("api.urls"))
            {
                app.Urls.Add(url.Value<string>()!);
            }

            app.MapPut("/signin", async (HttpContext ctx, string username, string password) =>
            {
                var u = Connector_User.Basic(username, password);
                Logger.Info(username + " failed login attempt.");
                if (u is null) { return false.ToJson(); }

                var claims = new List<Claim>()
                {
                    new Claim("name", u.Username), 
                    new Claim("id", u.ID.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(6),
                    IsPersistent = true,
                };

                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity), authProperties);

                Logger.Info(username + " logged in.");

                return true.ToJson();
            });

            app.MapGet("/signout.html", async (HttpContext ctx) =>
            {
                await ctx.SignOutAsync();
                return Results.Redirect("/");
            });

            app.MapGet("/api/ping", () => "pong").RequireAuthorization();

            app.MapGet("/api/items", (HttpContext ctx, string realm) => 
                Connector_Item.GetAll(realm, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapGet("/api/item", (HttpContext ctx, string id) => 
                Connector_Item.GetByID(id, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapDelete("/api/item", (HttpContext ctx, string id) => 
                Connector_Item.Delete(id, ctx.Caller())).RequireAuthorization();
            app.MapPost("/api/item", (HttpContext ctx, Item item) => 
                Connector_Item.Add(item, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapPut("/api/item", (HttpContext ctx, Item item) => 
                Connector_Item.Update(item, ctx.Caller()).ToJson()).RequireAuthorization();

            app.MapGet("/api/realms", (HttpContext ctx) => 
                Connector_Realm.GetAll(ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapGet("/api/realm", (HttpContext ctx, string id) => 
                Connector_Realm.GetByID(id, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapPost("/api/realm", (HttpContext ctx, Realm realm) => 
                Connector_Realm.Create(realm, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapPut("/api/realm", (HttpContext ctx, Realm realm) =>
                Connector_Realm.Update(realm, ctx.Caller()).ToJson()).RequireAuthorization();

            app.MapGet("/api/realm/user", (string realm, string user) => 
                Connector_Realm.GetPermission(user, realm).ToJson()).RequireAuthorization();
            app.MapGet("/api/realm/users", (string realm) =>
                Connector_Realm.GetAllPermissions(realm).ToJson()).RequireAuthorization();
            app.MapPut("/api/realm/user", (HttpContext ctx, string realm, string user, int permission) => 
                Connector_Realm.SetPermission(user, realm, (Permissions)permission, ctx.Caller())).RequireAuthorization();

            app.MapGet("/api/users", () => 
                Connector_User.GetAll().Select(u => u.ToFriendly()).ToJson()).RequireAuthorization();
            app.MapGet("/api/user", (HttpContext ctx) => 
                Connector_User.GetAll().FirstOrDefault(u => u.ID == ctx.Caller())!.ToFriendly().ToJson()).RequireAuthorization();
            app.MapPost("/api/user", (HttpContext ctx, string username, string email) =>
                Connector_User.CreateUser(username, email, ctx.Caller()));
            app.MapPut("/api/user/password", (HttpContext ctx, string oldPass, string newPass) => 
                Connector_User.ChangePassword(ctx.Caller(), oldPass, newPass).ToJson()).RequireAuthorization();
            app.MapPut("/api/user/email", (HttpContext ctx, string user, string email) =>
                Connector_User.ChangeEmail(user, email, ctx.Caller()).ToJson()).RequireAuthorization();
            app.MapPut("/api/user/reset", (HttpContext ctx, string user) =>
                Connector_User.ResetPassword(user, ctx.Caller()).ToJson()).RequireAuthorization();

            static string GetHistory(HttpContext ctx, string realm = null!, int page = 1, string source = null!)
            {
                return Connector_History.Get(page, ctx.Caller(), realm, source).ToJson();
            }
            app.MapGet("/api/realm/history", GetHistory).RequireAuthorization();

            static string GetHistorySize(HttpContext ctx, string realm = null!, string source = null!)
            {
                return Connector_History.GetSize(ctx.Caller(), realm, source).ToJson();
            }
            app.MapGet("/api/realm/historysize", GetHistorySize).RequireAuthorization();

            app.MapGet("/api/ids", (int count) =>
            {
                count = count > 1000 ? 1000 : count < 0 ? 0 : count;
                return Enumerable.Range(0, count).Select(_ => Snowflake.Generate(1).ToString()).ToArray().ToJson();
            }).RequireAuthorization();

            _ = app.RunAsync();
        }
    }
}
