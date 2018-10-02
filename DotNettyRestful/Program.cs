namespace DotNettyRestful
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using DotNetty.Codecs.Http;
    using DotNetty.Transport.Bootstrapping;
    using DotNetty.Transport.Channels;
    using DotNetty.Transport.Channels.Sockets;
    using DotNettyRestful.Handler;

    class Program
    {
        static void Main(string[] args)
        {
            StartServer(8080).Wait();
        }

        static async Task StartServer(int port)
        {
            IEventLoopGroup bossGroup = new MultithreadEventLoopGroup(1);
            IEventLoopGroup workerGroup = new MultithreadEventLoopGroup();
            var bootstrap = new ServerBootstrap();
            bootstrap.Group(bossGroup, workerGroup);
            bootstrap.Channel<TcpServerSocketChannel>();

            bootstrap
                .Option(ChannelOption.SoBacklog, 8192)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new HttpServerCodec());
                    pipeline.AddLast(new HttpObjectAggregator(64*1024));
                    pipeline.AddLast(new SampleRestfulHandler());
                }));

            IChannel bootstrapChannel = await bootstrap.BindAsync(IPAddress.Any, port);
            Console.WriteLine($"Httpd started. Listening on {bootstrapChannel.LocalAddress}");
            Console.ReadLine();

            await bootstrapChannel.CloseAsync();
        }
    }
}
