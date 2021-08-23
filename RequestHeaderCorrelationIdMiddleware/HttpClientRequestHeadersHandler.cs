using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RequestHeaderCorrelationId
{
    public class HttpClientRequestHeadersHandler : DelegatingHandler
    {
        public HttpClientRequestHeadersHandler(IHttpContextAccessor httpContextAccessor, ILogger<HttpClientRequestHeadersHandler> logger)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = logger;
        }

        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";
        private const string FROM_HEADER = "from";
        private const string ApplicationNameEnvironment = "ApplicationName";
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger<HttpClientRequestHeadersHandler> logger;
        private static string ApplicationName => Environment.GetEnvironmentVariable(ApplicationNameEnvironment);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            logger.LogInformation("SendAsync");
            if (!(!StringValues.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER])
                && Guid.TryParse(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
                logger.LogInformation($"SendAsync > Generating new id {correlationId}");
            }

            logger.LogInformation($"SendAsync > Using existent id {correlationId}");
            request.Headers.Add(CORRELATION_TOKEN_HEADER, correlationId.ToString());
            request.Headers.Add(FROM_HEADER, ApplicationName);
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
