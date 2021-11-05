using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Jnz.RequestHeaderCorrelationId
{
    public static class HttpContextAccessorExtensions
    {
        private static readonly string Correlation_Token_Header = "x-correlation-id";
        public static Guid? GetCorrelationId(this IHttpContextAccessor httpContextAccessor)
        {
            if (!(!StringValues.IsNullOrEmpty(httpContextAccessor.HttpContext.Request.Headers.ContainsKey(Correlation_Token_Header).ToString())
                && Guid.TryParse(httpContextAccessor.HttpContext.Request.Headers[Correlation_Token_Header], out Guid correlationId)))
                return null;

            return correlationId;
        }
    }
}
