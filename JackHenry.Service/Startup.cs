using Autofac;
using JackHenry.Service.Utilities;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(JackHenry.Service.Startup))]

namespace JackHenry.Service
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<TwitterConfigManager>().AsSelf()
                                                        .AsImplementedInterfaces()
                                                        .SingleInstance();
            builder.RegisterType<TwitterStreamService>().As<ITwitterStreamService>();
            app.Use(builder);
        }
    }
}
