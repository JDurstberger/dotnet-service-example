using System.Net;
using Shared;
using Shared.Service;
using Endpoint = Shared.Service.Endpoint;

namespace DotnetServiceExample.Echo;

internal static class Handlers
{
    internal static readonly Func<HttpContext, Task<Response>> GetEcho = context =>
    {
        var resource = HalResource.Create()
            .AddProperty("echo", "Hello");
        return Task.FromResult(new Response(HttpStatusCode.OK, resource));
    };
    
    internal static readonly Func<HttpContext, Task<Response>> CreateEcho = async context =>
    {
        var body = await ReadBodyAsString(context);
        var resource = HalResource.Create()
            .AddProperty("echo", body);
        
        return new Response(HttpStatusCode.OK, resource);
    };
    
    private static async Task<string> ReadBodyAsString(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body);
        return await reader.ReadToEndAsync();
    }
}

public static class Endpoints
{
    public static List<Endpoint> All => [GetEcho, CreateEcho];

    internal static readonly Endpoint GetEcho = new(
        HttpMethod.Get,
        "/",
        Handlers.GetEcho
    );
    
    internal static readonly Endpoint CreateEcho = new(
        HttpMethod.Post,
        "/",
        Handlers.CreateEcho
    );
}