using Shared.Service;
using Endpoint = Shared.Service.Endpoint;

namespace SomethingElse;

public static class Attach
{
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