using System;
using System.Collections.Generic;
using OpenTracing;
using OpenTracing.Tag;

namespace Byndyusoft.Tracing
{
    public static class SpanExtensions
    {
        public static void SetException(this ISpan span, Exception exception)
        {
            if (span == null || exception == null)
                return;

            span.SetTag(Tags.Error, true);

            span.Log(new Dictionary<string, object>(3)
            {
                {LogFields.Event, Tags.Error.Key},
                {LogFields.ErrorKind, exception.GetType().Name},
                {LogFields.ErrorObject, exception}
            });
        }
    }
}