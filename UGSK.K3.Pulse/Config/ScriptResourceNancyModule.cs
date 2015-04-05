using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Owin;

namespace UGSK.K3.Pulse.Config
{
    public class ScriptResourceNancyModule : NancyModule
    {
        public ScriptResourceNancyModule()
        {
            Get["sales-statistic"] = parameters =>
            {
                var owinEnv = Context.GetOwinEnvironment();

                var requestHeaders = (IDictionary<string, string[]>)owinEnv["owin.RequestHeaders"];

                var uri = string.Format("{0}://{1}{2}", owinEnv["owin.RequestScheme"], requestHeaders["Host"].First(),
                    owinEnv["owin.RequestPathBase"]);

                var env = new StatisticGainerEnvironment { ServiceAddress = uri };
                return Negotiate
                    .WithModel(env)
                    .WithHeader("Content-Type", "text/javascript")
                    .WithMediaRangeModel("text/javascript", env)
                    .WithView("statistic");
            };
        }
    }
}