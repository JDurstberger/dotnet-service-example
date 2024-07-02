using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Shared;
using Shared.Service;
using Endpoint = Shared.Service.Endpoint;

namespace DotnetServiceExample;

public class App
{
    private static readonly IEnumerable<Endpoint> Endpoints = Echo.Endpoints.All;

    private readonly WebApplication _app;

    public App()
    {
        var builder = WebApplication.CreateSlimBuilder();
        ConfigureHosting(builder);
        ConfigureWebApplication(builder);

        _app = builder.Build();
        SetMiddlewares(_app);
        BaseApp.AttachRoutes(_app, Endpoints);
    }

    private static void ConfigureHosting(WebApplicationBuilder builder)
    {
        builder.Services.AddAWSLambdaHosting(
            LambdaEventSource.RestApi,
            new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>()
        );
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolver = LambdaFunctionJsonSerializerContext.Default);

        builder.WebHost.UseUrls($"http://localhost:9999");
    }
    
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
    
    public Task Run()
    {
        return _app.RunAsync();
    }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext;