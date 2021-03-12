using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using OpenTracing.Contrib.NetCore.AspNetCore;

namespace Byndyusoft.Tracing
{
    /// <summary>
    ///     Расширения для конфигурирования настроек трассировки запросов в ASP .NET Core
    /// </summary>
    public static class HostingOptionsExtensions
    {
        private const string MetricsUri = "/metrics";
        private const string SwaggerUri = "/swagger";
        private const string HealthUri = "/healthz";
        private const string FaviconUri = "/favicon";

        private static readonly Regex UrlValuesMatcher;

        static HostingOptionsExtensions()
        {
            UrlValuesMatcher = new Regex(@"(\/)\d+", RegexOptions.Compiled);
        }

        /// <summary>
        ///     Добавляет делегат для определения запросов, которые не должны попасть в трассировку
        /// </summary>
        /// <param name="hostingOptions">Настройки трассировки запросов в ASP .NET Core</param>
        /// <param name="pattern">Делегат, проверяющий следует ли добавлять запрос в трассировку</param>
        /// <returns>>Настройки трассировки запросов в ASP .NET Core</returns>
        public static HostingOptions AddIgnorePattern(this HostingOptions hostingOptions,
            Func<HttpContext, bool> pattern)
        {
            if (hostingOptions == null)
                throw new ArgumentNullException(nameof(hostingOptions));
            if (pattern == null)
                throw new ArgumentNullException(nameof(pattern));

            hostingOptions.IgnorePatterns.Add(pattern);
            return hostingOptions;
        }

        /// <summary>
        ///     Добавляет делегат для игнорирования запросов, путь которых содержит требуемый фрагмент
        /// </summary>
        /// <param name="hostingOptions">Настройки трассировки запросов в ASP .NET Core</param>
        /// <param name="uri">Фрагмент пути запросов, которые следует игнорировать</param>
        /// <returns>Настройки трассировки запросов в ASP .NET Core</returns>
        public static HostingOptions AddUriIgnorePattern(this HostingOptions hostingOptions, string uri)
        {
            return hostingOptions.AddIgnorePattern(
                context => context.Request.Path.Value.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        /// <summary>
        ///     Заменяет делегат, генерирующий имя корневого спана
        /// </summary>
        /// <param name="hostingOptions">Настройки трассировки запросов в ASP .NET Core</param>
        /// <param name="operationNameResolver">Делегат, устанавливающий имя корневого спана</param>
        /// <returns>Настройки трассировки запросов в ASP .NET Core</returns>
        public static HostingOptions WithOperationNameResolver(
            this HostingOptions hostingOptions,
            Func<HttpContext, string> operationNameResolver
        )
        {
            hostingOptions.OperationNameResolver = operationNameResolver;
            return hostingOptions;
        }

        /// <summary>
        ///     Добавляет стандартные делегаты для игнорирования запросов
        /// </summary>
        /// <param name="hostingOptions">Настройки трассировки запросов в ASP .NET Core</param>
        /// <returns>Настройки трассировки запросов в ASP .NET Core</returns>
        public static HostingOptions AddDefaultIgnorePatterns(this HostingOptions hostingOptions)
        {
            return hostingOptions
                .AddIgnorePattern(context => context.Items.Any())
                .AddUriIgnorePattern(MetricsUri)
                .AddUriIgnorePattern(SwaggerUri)
                .AddUriIgnorePattern(HealthUri)
                .AddUriIgnorePattern(FaviconUri);
        }

        /// <summary>
        ///     Устанавливает стандартный делегат, генерирующий имя корневого спана
        /// </summary>
        /// <param name="hostingOptions">Настройки трассировки запросов в ASP .NET Core</param>
        /// <param name="additionalPatterns">Дополнительные правила замены частей урла</param>
        /// <returns>Настройки трассировки запросов в ASP .NET Core</returns>
        public static HostingOptions WithDefaultOperationNameResolver(
            this HostingOptions hostingOptions,
            params Regex[] additionalPatterns
        )
        {
            var patterns = additionalPatterns.Append(UrlValuesMatcher).ToArray();
            return hostingOptions.WithOperationNameResolver(
                context =>
                    $"{context.Request.Method} {patterns.Aggregate(context.Request.Path.Value, (path, pattern) => pattern.Replace(path, "$1#val"))}"
            );
        }
    }
}