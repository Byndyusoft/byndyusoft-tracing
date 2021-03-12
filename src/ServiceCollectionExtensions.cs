using System;
using System.Reflection;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Contrib.NetCore.AspNetCore;
using OpenTracing.Util;

namespace Byndyusoft.Tracing
{
    public static class ServiceCollectionExtensions
    {
        private const string JaegerTraceUri = "/api/traces";

        public static IServiceCollection AddJaegerTracer(
            this IServiceCollection services,
            IConfiguration configuration,
            Assembly rootAssembly = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return services.AddSingleton<ITracer>(
                serviceProvider =>
                {
                    var assembly = rootAssembly ?? Assembly.GetEntryAssembly();
                    var serviceName = assembly!.GetName().Name;
                    var serviceVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                        .InformationalVersion;

                    var hostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                    Configuration.SenderConfiguration.DefaultSenderResolver =
                        new SenderResolver(loggerFactory).RegisterSenderFactory<ThriftSenderFactory>();
                    var senderConfig =
                        Configuration.SenderConfiguration.FromIConfiguration(loggerFactory,
                            configuration.GetSection("Jaeger"));

                    var tracer = new Tracer.Builder(serviceName)
                        .WithTag("service.version", serviceVersion)
                        .WithTag("service.environment_name", hostingEnvironment?.EnvironmentName ?? "unknown")
                        .WithLoggerFactory(loggerFactory)
                        .WithExpandExceptionLogs()
                        .WithReporter(
                            new RemoteReporter.Builder()
                                .WithLoggerFactory(loggerFactory)
                                .WithSender(senderConfig.GetSender())
                                .Build()
                        )
                        .WithSampler(new ConstSampler(true))
                        .Build();

                    GlobalTracer.RegisterIfAbsent(tracer);

                    return tracer;
                });
        }

        public static IServiceCollection AddOpenTracingServices(
            this IServiceCollection services,
            Action<HostingOptions> configure
        )
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddOpenTracingCoreServices(
                builder => builder
                    .AddLoggerProvider()
                    .AddHttpHandler(
                        options => options.IgnorePatterns.Add(request =>
                            request.RequestUri.AbsolutePath.EndsWith(JaegerTraceUri))
                    )
                    .AddSystemSqlClient()
                    .AddAspNetCore(
                        options =>
                        {
                            options.LogEvents = false;
                            configure(options.Hosting);
                        }
                    )
            );
        }

        public static IServiceCollection AddOpenTracingServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddOpenTracingCoreServices(
                builder => builder
                    .AddLoggerProvider()
                    .AddHttpHandler(
                        options => options.IgnorePatterns.Add(request =>
                            request.RequestUri.AbsolutePath.EndsWith(JaegerTraceUri))
                    )
                    .AddSystemSqlClient()
            );
        }
    }
}