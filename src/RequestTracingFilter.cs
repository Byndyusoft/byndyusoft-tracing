using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseExtensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using OpenTracing;

namespace Byndyusoft.Tracing
{
    /// <summary>
    ///     Фильтр, добавляющий в активный span запрос к сервису
    /// </summary>
    public class RequestTracingFilter : IAsyncActionFilter
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly ITracer _tracer;

        /// <summary>
        ///     Конструктор для инициализации зависимостей фильтра
        /// </summary>
        /// <param name="tracer">Трассировщик</param>
        public RequestTracingFilter(ITracer tracer)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _jsonSerializerSettings = new JsonSerializerSettings().ApplyDefaultSettings();
        }

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.Count > 0)
                _tracer.ActiveSpan?.Log(PrepareLogEntry(context.ActionArguments));

            await next();
        }

        private static string GenerateMessageTemplate(IDictionary<string, object> actionArguments)
        {
            return
                $"Action arguments: {string.Join(", ", actionArguments.Keys.Select(x => $"{x} {{{x.ToPascalCase()}}}"))}";
        }

        private IReadOnlyDictionary<string, object> PrepareLogEntry(IDictionary<string, object> actionArguments)
        {
            var logEntryFields
                = new Dictionary<string, object>
                {
                    [LogFields.Event] = "Action executing",
                    [$"{LogFields.Message}.template"] = GenerateMessageTemplate(actionArguments)
                };
            foreach (var (actionArgumentName, actionArgumentValue) in actionArguments)
                logEntryFields[actionArgumentName.ToPascalCase()] =
                    JsonConvert.SerializeObject(actionArgumentValue, _jsonSerializerSettings);

            return logEntryFields;
        }
    }
}