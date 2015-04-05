using System;
using Hangfire;
using LightInject;

namespace UGSK.K3.Pulse.Config
{
    public class ContainerJobActivator : JobActivator
    {
        private ServiceContainer _container;

        public ContainerJobActivator(ServiceContainer container)
        {
            _container = container;
        }

        public override object ActivateJob(Type type)
        {
            using (_container.BeginScope())
            {
                return _container.GetInstance(type);
            }
        }
    }
}