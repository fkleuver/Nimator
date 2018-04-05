using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Nimator.Util;

namespace Nimator.Web.Middlewares
{
    public sealed class SPABootstrapperSettings
    {
        public SPABootstrapperSettings(string bootstrapUri, Dictionary<string, string> bootstrapInformation)
        {
            BootstrapUri = bootstrapUri;
            BootstrapInformation = bootstrapInformation;
        }

        public SPABootstrapperSettings(string bootstrapUri) : this(bootstrapUri, new Dictionary<string, string>())
        {
        }

        public string BootstrapUri { get; set; }
        public Dictionary<string, string> BootstrapInformation { get; set; }

        public SPABootstrapperSettings WithInformation(string key, string value)
        {
            BootstrapInformation.Add(key, value);
            return this;
        }
    }

    public sealed class SPABootstrapperMiddleware : OwinMiddleware
    {
        private readonly SPABootstrapperSettings _bootstrapperSettings;

        public SPABootstrapperMiddleware(OwinMiddleware next, SPABootstrapperSettings bootstrapperSettings) : base(next)
        {
            Guard.AgainstNull(nameof(bootstrapperSettings), bootstrapperSettings);
            _bootstrapperSettings = bootstrapperSettings;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var requestpath = context.Request.Path.Value;

            if (requestpath != _bootstrapperSettings.BootstrapUri)
            {
                await Next.Invoke(context);
            }
            else
            {
                var serializer = new JsonSerializer();
                using (var stream = new MemoryStream())
                using (var streamWriter = new StreamWriter(stream))
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    serializer.Serialize(jsonWriter, _bootstrapperSettings.BootstrapInformation);
                    jsonWriter.Flush();
                    stream.Position = 0;
                    context.Response.Headers.Set("Content-Type", "application/json");
                    await stream.CopyToAsync(context.Response.Body);
                }

            }
        }
    }
}
