using System.Web.Http;
using PositiveTechnologiesProblems.Domain;

namespace PositiveTechnologiesProblems.Agent2
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
