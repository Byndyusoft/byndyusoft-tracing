namespace Byndyusoft.Tracing
{
    using System;
    using MaskedSerialization.Newtonsoft.Helpers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal static class JsonSerializerSettingsExtensions
    {
        public static JsonSerializerSettings ApplyDefaultSettings(this JsonSerializerSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            MaskedSerializationHelper.SetupSettingsForMaskedSerialization(settings);
            settings.Converters.Add(new StringEnumConverter());
            settings.TypeNameHandling = TypeNameHandling.Auto;
            return settings;
        }
    }
}