using System;
using System.Reflection;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Nimator.Web.Util
{
    public class SignalRContractResolver : IContractResolver
    {
        private readonly Assembly _assembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRContractResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _assembly = typeof(Connection).Assembly;
        }

        public JsonContract ResolveContract(Type type)
        {
            if (type.Assembly.Equals(_assembly))
            {
                return _defaultContractSerializer.ResolveContract(type);
            }

            return _camelCaseContractResolver.ResolveContract(type);
        }

        public static void Configure()
        {
            var jsonSettings = new JsonSerializerSettings { ContractResolver = new SignalRContractResolver() };
            jsonSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            jsonSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            var serializer = JsonSerializer.Create(jsonSettings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
        }

    }
}
