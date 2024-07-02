using System.Net;

using Microsoft.AspNetCore.Http;

namespace Shared.Service;

public record Response(HttpStatusCode StatusCode, HalResource Resource);
public record Endpoint(HttpMethod Method, string Route, Func<HttpContext, Task<Response>> Handler);