using LightInject;
using LightInject.Nancy;
using Nancy;
using Nancy.Bootstrapper;

namespace UGSK.K3.Pulse.Config
{
    // ReSharper disable once UnusedMember.Global
    public class Bootstrapper : LightInjectNancyBootstrapper
    {
        protected override IServiceContainer GetServiceContainer()
        {
            return base.GetServiceContainer();
        }

        protected override void ApplicationStartup(IServiceContainer container, IPipelines pipelines)
        {
            StaticConfiguration.DisableErrorTraces = false;
            base.ApplicationStartup(container, pipelines);
        }
    }
}