using System.Net;
using System.Text.Json.Serialization;

using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;

namespace DotnetServiceExample; 

public class App
{
  private readonly WebApplication _app;

  public App()
  {
    var builder = WebApplication.CreateSlimBuilder();
    ConfigureHosting(builder);
    ConfigureLogger(builder);

    _app = builder.Build();
    AttachRoutes(_app);
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

  private static void ConfigureLogger(WebApplicationBuilder builder)
  {
    builder.Logging.AddJsonConsole(options =>
    {
      options.UseUtcTimestamp = true;
      options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
    });
  }

  private void AttachRoutes(IEndpointRouteBuilder app)
  {
    app.MapGet("/", (Delegate)(async (HttpContext context) => await BaseHandler.Handle(_app, context)));
    app.MapPost("/", (Delegate)(async (HttpContext context) => await BaseHandler.Handle(_app, context)));
  }

  public Task Run()
  {
    return _app.RunAsync();
  }
}

public static class BaseHandler
{
  public static async Task<string> Handle(WebApplication app, HttpContext context)
  {
    try
    {
      var body = await ReadBodyAsString(context);
      context.Response.ContentType = "application/hal+json";
      context.Response.StatusCode = (int)HttpStatusCode.OK;
      return body;
    }
    catch (Exception e)
    {
      app.Logger.LogError(e, "unexpected exception occured");
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      return "{}";
    }
  }
  
  private static async Task<string> ReadBodyAsString(HttpContext context)
  {
    using var reader = new StreamReader(context.Request.Body);
    return await reader.ReadToEndAsync();
  }
}

[JsonSerializable(typeof(APIGatewayProxyRequest))]
[JsonSerializable(typeof(APIGatewayProxyResponse))]
[JsonSerializable(typeof(string))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext;