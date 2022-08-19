using API.Entities;
using API.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using Serilog.Events;
using System.Dynamic;

namespace API.Middlewares
{
    public class LogMiddleware : IMiddleware
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;

        public LogMiddleware(IHttpContextAccessor httpContextAccessor,
                             IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            HttpContext _httpContext = _httpContextAccessor.HttpContext;

            if (IsIgnoredPath(_httpContext.Request.Path))
            {
                await next(context);
                return;
            }

            context.Request.EnableBuffering();

            string method = _httpContext.Request.Method;
            string route = _httpContext.Request.Path + _httpContext.Request.QueryString.Value;

            Log.Information("Chamando o endpoint [{method:l}] {route:l}", method, route);

            string payload = await GetPayload(_httpContext.Request);

            if (!string.IsNullOrEmpty(payload))
                Log.Information("Payload: \n{payload}", payload);

            await next(context);

            int statusCode = _httpContext.Response.StatusCode;

            Log.Information("Finalizado com StatusCode {statusCode}", statusCode);

            try
            {
                ManageLogs();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool IsIgnoredPath(PathString path)
        {
            string[] ignoredPaths =
            {
                "/swagger/index.html",
                "/swagger/v1/swagger.json",
            };

            return ignoredPaths.Contains(path.Value);
        }

        private async Task<string> GetPayload(HttpRequest request)
        {
            string contentType = request.ContentType ?? string.Empty;

            if (!contentType.Contains("application/json"))
                return string.Empty;

            StreamReader reader = new(request.Body);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            string body = await reader.ReadToEndAsync();

            request.Body.Position = 0;

            return body;
        }

        private async void ManageLogs()
        {
            string traceId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;

            if (string.IsNullOrEmpty(traceId))
                return;

            if (_memoryCache.Get<TraceContainer>(traceId) is not TraceContainer traceContainer)
                return;

            _memoryCache.Remove(traceId);

            await Task.Run(async () =>
            {
                await SaveTraceContainer(traceContainer);

                Console.WriteLine($"Log registrado: {traceId}");
            });
        }

        private async Task SaveTraceContainer(TraceContainer traceContainer)
        {
            await Task.Delay(3000);

            Trace trace = new()
            {
                Id = traceContainer.TraceId,
                Logs = traceContainer.LogEvents.Select(logEvent =>
                {
                    TraceLog traceLog = new()
                    {
                        Level = logEvent.Level.ToString(),
                        Message = logEvent.RenderMessage(),
                        Exception = logEvent.Exception?.ToString(),
                        At = logEvent.Timestamp.DateTime,
                    };

                    return traceLog;
                }).ToList()
            };

            // Utilize este método para salvar os dados no banco, como por exemplo:
            // _mongoRepository.Save(trace);
        }
    }
}
