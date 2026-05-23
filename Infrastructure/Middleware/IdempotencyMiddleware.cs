using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Sales.API.Infrastructure.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, (int StatusCode, string Body)> Cache = new();

    public IdempotencyMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // if (context.Request.Method != HttpMethods.Post && context.Request.Method != HttpMethods.Put)
        // {
        //     await _next(context);
        //     return;
        // }

        // if (context.Request.Path.Value?.EndsWith("/payment", StringComparison.OrdinalIgnoreCase) == true)
        // {
        //     await _next(context);
        //     return;
        // }

        // if (context.Request.Method == HttpMethods.Post &&
        //     context.Request.Path.Value?.EndsWith("/tickets", StringComparison.OrdinalIgnoreCase) == true)
        // {
        //     await _next(context);
        //     return;
        // }
        await _next(context);
        // context.Request.EnableBuffering();
        // using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        // var body = await reader.ReadToEndAsync();
        // context.Request.Body.Position = 0;

        // var keySource = context.Request.Path + "|" + body;
        // var key = ComputeHash(keySource);

        // if (Cache.TryGetValue(key, out var cached))
        // {
        //     context.Response.StatusCode = cached.StatusCode;
        //     context.Response.ContentType = "application/json";
        //     await context.Response.WriteAsync(cached.Body);
        //     return;
        // }

        // var originalBody = context.Response.Body;
        // try
        // {
        //     using var memStream = new MemoryStream();
        //     context.Response.Body = memStream;
        //     await _next(context);
        //     context.Response.Body.Seek(0, SeekOrigin.Begin);
        //     var respBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        //     context.Response.Body.Seek(0, SeekOrigin.Begin);

        //     if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        //     {
        //         Cache[key] = (context.Response.StatusCode, respBody);
        //     }

        //     await memStream.CopyToAsync(originalBody);
        // }
        // finally
        // {
        //     context.Response.Body = originalBody;
        // }
    }

    private static string ComputeHash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input ?? string.Empty));
        return Convert.ToHexString(bytes);
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseSimpleIdempotency(this IApplicationBuilder app)
        => app.UseMiddleware<IdempotencyMiddleware>();
}
