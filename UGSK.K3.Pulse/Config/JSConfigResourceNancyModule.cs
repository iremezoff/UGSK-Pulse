using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Owin;

namespace UGSK.K3.Pulse.Config
{
    public class JSConfigResourceNancyModule : NancyModule
    {
        public JSConfigResourceNancyModule()
        {
            Get["config"] = parameters =>
            {
                var owinEnv = Context.GetOwinEnvironment();

                var requestHeaders = (IDictionary<string, string[]>)owinEnv["owin.RequestHeaders"];

                var uri = string.Format("{0}://{1}{2}", owinEnv["owin.RequestScheme"], requestHeaders["Host"].First(),
                    owinEnv["owin.RequestPathBase"]);

                var env = new
                {
                    ServiceAddress = uri,
                    DailyPeriod = (int)PeriodKind.Daily,
                    WeeklyPeriod = (int)PeriodKind.Weekly,
                    TotalCounter = (int)CounterKind.Total,
                    AverageCounter = (int)CounterKind.Average
                };
                return Negotiate
                    .WithModel(env)
                    .WithHeader("Content-Type", "text/javascript")
                    .WithMediaRangeModel("text/javascript", env)
                    .WithView("config");
            };
        }
    }
}