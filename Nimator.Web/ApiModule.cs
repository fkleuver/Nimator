using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Nimator.Util;
using Nimator.Web.Util;

namespace Nimator.Web
{
    public sealed class ApiModule : Module
    {
        private readonly HttpConfiguration _config;

        public ApiModule(HttpConfiguration config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterLogProvider();
            builder.RegisterWebApiFilterProvider(_config);
            builder.RegisterApiControllers(GetType().Assembly);

            builder.Register(c => AppSettings.FromConfigurationManager())
                .AsImplementedInterfaces()
                .SingleInstance();

 
        }
    }
}
