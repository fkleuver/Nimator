using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Nimator.Util
{
    public static class LogSerializer
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };

        static LogSerializer()
        {
            JsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
        }

        public static string Serialize([NotNull]object logObject)
        {
            return JsonConvert.SerializeObject(logObject, JsonSettings);
        }
    }
}
