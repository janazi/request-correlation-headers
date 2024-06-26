﻿using System;
using Jnz.RequestHeaderCorrelationId;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Jnz.RequestHeaderCorrelationIdTests
{
    public class HttpContextAccessorExtensionsTests
    {
        private const string CORRELATION_TOKEN_HEADER = "x-correlation-id";

        [Fact]
        public void Should_ReturnGuid_FromHeader()
        {
            var correlationId = Guid.NewGuid().ToString();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);
            context.Request.Headers.Append(CORRELATION_TOKEN_HEADER, correlationId);

            var guidFromHeader = mockHttpContextAccessor.Object.GetCorrelationId();

            Assert.Equal(correlationId, guidFromHeader.ToString());
        }

        [Fact]
        public void Should_ReturnNull_FromHeaderWithoutGuid()
        {
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var context = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

            var guidFromHeader = mockHttpContextAccessor.Object.GetCorrelationId();

            Assert.Null(guidFromHeader);
        }
    }
}
