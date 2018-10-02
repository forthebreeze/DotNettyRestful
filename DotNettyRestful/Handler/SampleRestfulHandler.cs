namespace DotNettyRestful.Handler
{
    using System;
    using System.Text;
    using DotNetty.Buffers;
    using DotNetty.Codecs.Http;
    using DotNetty.Common.Utilities;
    using DotNetty.Transport.Channels;
    using DotNettyRestful.Model;
    using Newtonsoft.Json;

    public class SampleRestfulHandler : ChannelHandlerAdapter
    {
        private const string restfulRoute = "/samplePost";
        private void ReturnBadRequest(IChannelHandlerContext ctx)
        {
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.BadRequest, Unpooled.Empty, false);
            ctx.WriteAndFlushAsync(response);
            ctx.CloseAsync();
        }

        private void ReturnInternalServerError(IChannelHandlerContext ctx)
        {
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.InternalServerError, Unpooled.Empty, false);
            ctx.WriteAndFlushAsync(response);
            ctx.CloseAsync();
        }

        void WriteResponse(IChannelHandlerContext ctx, IByteBuffer buf, ICharSequence contentType, ICharSequence contentLength)
        {
            var response = new DefaultFullHttpResponse(HttpVersion.Http11, HttpResponseStatus.OK, buf, false);
            HttpHeaders headers = response.Headers;
            headers.Set(HttpHeaderNames.ContentType, contentType);
            headers.Set(HttpHeaderNames.ContentLength, contentLength);
            ctx.WriteAndFlushAsync(response);
            ctx.CloseAsync();
        }

        public override void ChannelRead(IChannelHandlerContext ctx, object message)
        {
            if (message is IFullHttpRequest httpRequest)
            {
                var succeeded = false;
                if (httpRequest.Method == HttpMethod.Post && restfulRoute.Equals(httpRequest.Uri, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (httpRequest.Content.HasArray)
                        {
                            String jsonStr = Encoding.UTF8.GetString(httpRequest.Content.Array, httpRequest.Content.ArrayOffset, httpRequest.Content.ReadableBytes);
                            SampleRequest request = JsonConvert.DeserializeObject<SampleRequest>(jsonStr);
                            SampleResponse sampleResponse = new SampleResponse() {Response =  request.Request };
                            var bytes =  Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sampleResponse));
                            WriteResponse(ctx, Unpooled.WrappedBuffer(bytes), AsciiString.Cached("application/json"), new AsciiString(bytes.Length.ToString()));
                        }
                    }
                    catch (Exception)
                    {
                        ReturnInternalServerError(ctx);
                    }
                }

                if (!succeeded)
                {
                    ReturnBadRequest(ctx);
                }
            }
            else
            {
                ctx.FireChannelRead(message);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
    }
}
