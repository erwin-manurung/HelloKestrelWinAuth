using System.Globalization;

namespace HelloKestrelWinAuth.Middlewares
{
    public interface ISimpleAuthMiddleware
    {
        Task InvokeAsync(HttpContext context, RequestDelegate next);
    }
    public class SimpleAuthMiddleware : ISimpleAuthMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var cultureQuery = context.Request.Query["culture"];
            if (!string.IsNullOrWhiteSpace(cultureQuery))
            {
                var culture = new CultureInfo(cultureQuery);

                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }

            // Call the next delegate/middleware in the pipeline.
            await next(context);
        }
    }
}
