using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Text.Json.Serialization;

namespace DotnetServiceExample;
public class Function
{
    private static async Task Main()
    {
        var app = new App();
        await app.Run();
    }
}