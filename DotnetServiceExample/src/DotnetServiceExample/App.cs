using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Shared.Service;
using SomethingElse;
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
        BaseApp.ConfigureWebApplication(builder);

        _app = builder.Build();
        BaseApp.SetMiddlewares(_app);
        Attach.AttachRoutes(_app, Endpoints);
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
    
    public Task Run()
    {
        return _app.RunAsync();
    }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext;