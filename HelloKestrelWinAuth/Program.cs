using HelloKestrelWinAuth.Middlewares;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.IISIntegration;
using System.Globalization;
using System.Net;
using System.Security.Claims;

public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ISimpleAuthMiddleware simpleAuthMiddleware = new SimpleAuthMiddleware();
        builder.Services.AddSingleton(simpleAuthMiddleware);

        // Add services to the container.

        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
        builder.Services.AddAuthorization(options =>
        {
        //    // By default, all incoming requests will be authorized according to the default policy.
            //options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddMvc((opt) =>
        {
            opt.AllowEmptyInputInBodyModelBinding = true;
        });
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");

        }

        //app.UseMiddleware<ISimpleAuthMiddleware>();
        app.UseStaticFiles();

        app.UseAuthentication();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.MapControllers();

        app.UseSwagger();
        app.UseSwaggerUI();

        app.Use(async (context, next) =>
        {
            var winIdentity = (ClaimsIdentity)context.User.Identity;
            Console.WriteLine($"Inside second middleware, isauthenticated : {winIdentity.IsAuthenticated}.\n");
            await next();
            Console.WriteLine("Exiting second middleware.\n");
        });
        app.Run();
    }
}