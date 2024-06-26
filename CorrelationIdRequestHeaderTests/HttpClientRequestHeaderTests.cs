﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jnz.RequestHeaderCorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Jnz.RequestHeaderCorrelationIdTests
{
    public class HttpClientRequestHeaderTests
    {
        private const string CorrelationTokenHeader = "x-correlation-id";
        private const string ApplicationNameEnvironment = "ApplicationName";

        public HttpClientRequestHeaderTests()
        {
            Environment.SetEnvironmentVariable(ApplicationNameEnvironment, "Tests");
        }

        [Fact]
        public async void ShouldCreate_CorrelationId()
        {
            Environment.SetEnvironmentVariable(ApplicationNameEnvironment, "Tests");
            var request = new HttpRequestMessage();
            var innerHandlerMock = new Mock<DelegatingHandler>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            innerHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", request, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { });

            var handler = new HttpClientRequestHeadersHandler(mockHttpContextAccessor.Object)
            {
                InnerHandler = innerHandlerMock.Object
            };

            var invoker = new HttpMessageInvoker(handler);
            var result = await invoker.SendAsync(request, default);

            Assert.NotNull(request.Headers.GetValues(CorrelationTokenHeader));
            var correlationId = Guid.Parse(request.Headers.GetValues(CorrelationTokenHeader).First());
            Assert.IsType<Guid>(correlationId);
        }

        [Fact]
        public async void ShouldNotChangeAnExisting_CorrelationId()
        {
            var correlationId = Guid.NewGuid().ToString();
            var request = new HttpRequestMessage();

            var innerHandlerMock = new Mock<DelegatingHandler>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            context.Request.Headers.Append(CorrelationTokenHeader, correlationId);

            innerHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", request, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { });

            var handler = new HttpClientRequestHeadersHandler(mockHttpContextAccessor.Object)
            {
                InnerHandler = innerHandlerMock.Object
            };

            var invoker = new HttpMessageInvoker(handler);
            var result = await invoker.SendAsync(request, default);
            var correlationIdReturned = context.Request.Headers[CorrelationTokenHeader];
            Assert.Equal(correlationId, correlationIdReturned);
        }
    }
}
