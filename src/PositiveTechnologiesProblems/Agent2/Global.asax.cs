using System.Threading.Tasks.Dataflow;
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
            var fdsmMessagingChain = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ITargetBlock<UpdateMessageDto>));
            fdsm.Init();
        }
    }
}
