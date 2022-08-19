using API.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog.Core;
using Serilog.Events;

namespace API.Enrichers
{
    public class TraceIdEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IMemoryCache _memoryCache;

        public TraceIdEnricher() 
        {
            _httpContextAccessor = new HttpContextAccessor();
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            string traceId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;

            if (string.IsNullOrEmpty(traceId))
                return;

            ConfigureCache();

            TraceContainer traceContainer = _memoryCache.GetOrCreate(traceId, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                return new TraceContainer
                {
                    TraceId = traceId,
                };
            });

            traceContainer.LogEvents.Add(logEvent);
        }

        private void ConfigureCache()
        {
            if (_memoryCache is not null)
                return;

            _memoryCache = 
                ServiceActivator.GetScope()
                                .ServiceProvider
                                .GetService<IMemoryCache>();
        }
    }
}
