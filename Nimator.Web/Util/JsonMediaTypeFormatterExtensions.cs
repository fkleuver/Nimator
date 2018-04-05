using System;
using System.Net.Http.Formatting;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Nimator.Logging;

namespace Nimator.Web.Util
{
    public static class JsonMediaTypeFormatterExtensions
    {
        public static void ConfigureCamelCase(
            this JsonMediaTypeFormatter formatter,
            ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Ignore,
            DateTimeZoneHandling dateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling nullValueHandling = NullValueHandling.Ignore,
            PreserveReferencesHandling preserveReferencesHandling = PreserveReferencesHandling.None,
            bool useDataContractJsonSerializer = false,
            bool useJsonMediaTypeForHtmlHeaderValue = true)
        {
            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            formatter.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });

            formatter.SerializerSettings.ReferenceLoopHandling = referenceLoopHandling;
            formatter.SerializerSettings.DateTimeZoneHandling = dateTimeZoneHandling;
            formatter.SerializerSettings.NullValueHandling = nullValueHandling;
            formatter.SerializerSettings.PreserveReferencesHandling = preserveReferencesHandling;
            formatter.UseDataContractJsonSerializer = useDataContractJsonSerializer;

            if (useJsonMediaTypeForHtmlHeaderValue)
            {
                formatter.MediaTypeMappings.Add(new RequestHeaderMapping("Accept", "text/html", StringComparison.InvariantCultureIgnoreCase, true, "application/json"));
            }
        }
        public static void AddLibLogErrorHandler(
            this JsonMediaTypeFormatter formatter,
            bool logErrorException = true,
            bool addErrorToHttpContext = true,
            bool rethrow = false)
        {
            formatter.SerializerSettings.Error += (sender, args) =>
            {
                if (logErrorException)
                {
                    var logger = HttpContext.Current.GetLogger();
                    logger.ErrorException("An error occured during JSON serialization", args.ErrorContext.Error);
                }
                if (addErrorToHttpContext)
                {
                    HttpContext.Current.AddError(args.ErrorContext.Error);
                }
                if (rethrow)
                {
                    throw args.ErrorContext.Error;
                }
            };
        }

    }
}
