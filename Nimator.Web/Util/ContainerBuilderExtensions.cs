using Autofac;
using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using Nimator.Logging;

namespace Nimator.Web.Util
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// Configures Autofac to resolve an instance of <see cref="ILog"/> using <see cref="LogProvider"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="loggerName"></param>
        public static void RegisterLogProvider(this ContainerBuilder builder, string loggerName = "LibLog")
        {
            builder.Register(c => LogProvider.GetLogger(loggerName)).AsImplementedInterfaces().InstancePerDependency();
        }

        /// <summary>
        /// Intercepts an interface registration with the <see cref="CallLogInterceptor"/> interceptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="registrationBuilder"></param>
        public static void InterceptWithCallLogger<T>(
            this IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> registrationBuilder)
        {
            registrationBuilder
                .EnableInterfaceInterceptors()
                .InterceptedBy(nameof(CallLogInterceptor));
        }
    }
}
