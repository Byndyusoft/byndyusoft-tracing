namespace Byndyusoft.Tracing
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings ApplyDefaultSettings(this JsonSerializerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            settings.Converters.Add(new StringEnumConverter());
            settings.TypeNameHandling = TypeNameHandling.Auto;
            return settings;
        }
    }
}