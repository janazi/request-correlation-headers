using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CorrelationIdRequestHeader
{
    public class HttpClientRequestHeadersHandler : DelegatingHandler
    {
        public HttpClientRequestHeadersHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";
        private readonly IHttpContextAccessor httpContextAccessor;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!(!StringValues.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER])
                && Guid.TryParse(httpContextAccessor.HttpContext.Request.Headers[CORRELATION_TOKEN_HEADER], out Guid correlationId)))
            {
                correlationId = Guid.NewGuid();
                request.Headers.TryAddWithoutValidation(CORRELATION_TOKEN_HEADER, correlationId.ToString());
            }

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
