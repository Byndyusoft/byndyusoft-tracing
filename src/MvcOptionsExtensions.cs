using Microsoft.AspNetCore.Mvc;

namespace Byndyusoft.Tracing
{
    /// <summary>
    ///     Расширения для настройки Mvc
    /// </summary>
    public static class MvcOptionsExtensions
    {
        /// <summary>
        ///     Добавляет фильтр для отправки запросов к сервису в трассировку
        /// </summary>
        public static MvcOptions PassRequestsToTracer(this MvcOptions mvcOptions)
        {
            mvcOptions.Filters.Add(typeof(RequestTracingFilter));
            return mvcOptions;
        }

        /// <summary>
        ///     Добавляет фильтр для отправки ответов сервиса в трассировку
        /// </summary>
        public static MvcOptions PassResponsesToTracer(this MvcOptions mvcOptions)
        {
            mvcOptions.Filters.Add(typeof(ResponseTracingFilter));
            return mvcOptions;
        }
    }
}