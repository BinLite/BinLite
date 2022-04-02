using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

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
            builder.Services.AddAuthorization();

            var certPath = Configuration.Get<string>("api.pfx");
            var certPass = Configuration.Get<string>("api.pfx-pw");
            if (!File.Exists(certPath))
            {
                throw new FileNotFoundException("Can't find provided api.pfx path.");
            }

            builder.WebHost.ConfigureKestrel((options) => {
                options.ConfigureHttpsDefaults(listenOptions => {
                    listenOptions.ServerCertificate = new X509Certificate2(certPath, certPass);
                });
            });

            var app = builder.Build();
            app.UseFileServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();

            foreach (var url in Configuration.Get<JArray>("api.urls"))
            {
                app.Urls.Add(url.Value<string>()!);
            }

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

            app.MapGet("/api/ids", (int count) =>
            {
                count = count > 1000 ? 1000 : count < 0 ? 0 : count;
                return Enumerable.Range(0, count).Select(_ => Snowflake.Generate(1).ToString()).ToArray().ToJson();
            }).RequireAuthorization();

            _ = app.RunAsync();
        }
    }
}
