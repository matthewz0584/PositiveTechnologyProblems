using System.Web.Http;
using PositiveTechnologies;

namespace Agent2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var fdsm = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFibonacciDistributedSequencesManager)) as IFibonacciDistributedSequencesManager;
            fdsm.Init();
        }
    }
}
