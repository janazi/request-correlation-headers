using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jnz.RequestHeaderCorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Jnz.RequestHeaderCorrelationIdTests
{
    public class RequestHeaderCorrelationIdMiddlewareTests
    {
        private const string CorrelationTokenHeader = "x-correlation-id";
        [Fact]
        public async Task CheckIfExistsCorrelationId_WhenHeadersContainXCorrelationId()
        {
            //ARRANGE
            var guid = Guid.NewGuid().ToString();
            var httpContextMoq = new Mock<HttpContext>();
            var headers = new Dictionary<string, StringValues>() {
                { "x-correlation-id", guid }
            };
            httpContextMoq.Setup(x => x.Request.Headers)
                .Returns(new HeaderDictionary(headers));

            var httpContext = httpContextMoq.Object;

            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));

            //ACT
            var middleware = new RequestHeaderCorrelationIdMiddleware(requestDelegate);
            await middleware.InvokeAsync(httpContext);

            //ASSERT
            Assert.False(StringValues.IsNullOrEmpty(httpContext.Request.Headers[CorrelationTokenHeader]));
            Assert.Equal(guid, httpContext.Request.Headers[CorrelationTokenHeader].ToString());
        }

        [Fact]
        public async Task CheckIfExistsCorrelationId_WhenHeadersDontContainXCorrelationId()
        {
            //ARRANGE
            var httpContextMoq = new Mock<HttpContext>();
            var headers = new Dictionary<string, StringValues>();
            httpContextMoq.Setup(x => x.Request.Headers)
                .Returns(new HeaderDictionary());

            var httpContext = httpContextMoq.Object;

            var requestDelegate = new RequestDelegate((innerContext) => Task.FromResult(0));

            //ACT
            var middleware = new RequestHeaderCorrelationIdMiddleware(requestDelegate);
            await middleware.InvokeAsync(httpContext);

            //ASSERT
            Assert.False(StringValues.IsNullOrEmpty(httpContext.Request.Headers[CorrelationTokenHeader]));
        }
    }
}
