using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Primitives;

namespace Jnz.RequestHeaderCorrelationId
{
    public class HttpClientRequestHeadersHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        private const string CorrelationIdHeader = "x-correlation-id";
        private const string UserAgentHeader = "User-Agent";
        private const string ApplicationNameEnvironment = "ApplicationName";
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private static string ApplicationName => Environment.GetEnvironmentVariable(ApplicationNameEnvironment) ??
            Environment.MachineName;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!(!StringValues.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Headers[CorrelationIdHeader])
                && Guid.TryParse(_httpContextAccessor.HttpContext.Request.Headers[CorrelationIdHeader], out Guid correlationId)))
                correlationId = Guid.NewGuid();

            request.Headers.Add(CorrelationIdHeader, correlationId.ToString());
            request.Headers.Add(UserAgentHeader, ApplicationName);
            return base.SendAsync(request, cancellationToken);
        }
    }

    public static class AddHttpClientRequestHeadersExtensions
    {
        /// <summary>
        /// Propagate the x-correlation-id headers to HttpClient requests
        /// </summary>
        /// <param name="services"></param>
        public static void PropagateCorrelationIdHeader(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<HttpClientRequestHeadersHandler>();
            services.ConfigureAll<HttpClientFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<HttpClientRequestHeadersHandler>());
                });
            });
        }
    }
}
