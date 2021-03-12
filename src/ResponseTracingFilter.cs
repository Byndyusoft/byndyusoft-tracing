using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using OpenTracing;

namespace Byndyusoft.Tracing
{
    /// <summary>
    ///     Фильтр, добавляющий в активный span ответ сервиса
    /// </summary>
    public class ResponseTracingFilter : IAsyncResultFilter
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ITracer _tracer;

        /// <summary>
        ///     Конструктор для инициализации зависимостей фильтра
        /// </summary>
        /// <param name="tracer">Трассировщик</param>
        public ResponseTracingFilter(ITracer tracer)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _jsonSerializerSettings = new JsonSerializerSettings().ApplyDefaultSettings();
        }

        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value != null)
                _tracer.ActiveSpan?.Log(
                    new Dictionary<string, object>
                    {
                        [LogFields.Event] = "Action result executing",
                        [$"{LogFields.Message}.template"] = "Response body {ResponseBody}",
                        ["ResponseBody"] = JsonConvert.SerializeObject(objectResult.Value, _jsonSerializerSettings)
                    }
                );

            await next();
        }
    }
}