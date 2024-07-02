using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Service;

public static class BaseApp
{
  public static void ConfigureWebApplication(
    WebApplicationBuilder builder
  )
  {
    ConfigureLogger(builder);
    BuildDependencies(builder);
  }

  private static void ConfigureLogger(WebApplicationBuilder builder)
  {
    builder.Logging.AddJsonConsole(options =>
    {
      options.UseUtcTimestamp = true;
      options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    });
  }

  private static void BuildDependencies(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<ILogger>(s => s.GetRequiredService<ILogger>());
  }

  public static void SetMiddlewares(IApplicationBuilder app)
  {
    app.UseStatusCodePages(async context =>
    {
      context.HttpContext.Response.ContentType = "application/hal+json";
      var resource = HalResource.Create().ToJson();
      await context.HttpContext.Response.WriteAsync("{}");
    });
  }

  public static void AttachRoutes(WebApplication app, IEnumerable<Endpoint> endpoints)
  {
    foreach (Endpoint endpoint in endpoints)
    {
      if (endpoint.Method == HttpMethod.Post)
      {
        app.MapPost(
          endpoint.Route,
          (Delegate)(async (HttpContext context) => await BaseHandler.Handle(app, endpoint, context))
        );
      }
      else
      {
        app.MapGet(
          endpoint.Route,
          (Delegate)(async (HttpContext context) => await BaseHandler.Handle(app, endpoint, context))
        );
      }
    }
  }
}