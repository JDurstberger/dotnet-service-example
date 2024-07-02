using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared.Service;

public static class BaseHandler
{
  public static async Task<string> Handle(WebApplication app, Endpoint endpoint, HttpContext context)
  {
    try
    {
      var response = await endpoint.Handler(context);
      context.Response.ContentType = "application/hal+json";
      context.Response.StatusCode = (int)response.StatusCode;
      return response.Resource.ToJson();
    }
    catch (Exception e)
    {
      app.Logger.LogError(e, "unexpected exception occured");
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      return "{}";
    }
  }
}