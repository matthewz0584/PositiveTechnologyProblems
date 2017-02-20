using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Agent2.Controllers;
using PositiveTechnologies;

namespace Agent2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost.fiddler:21005/") };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var ot = new OutboundHttpTransport(httpClient);

            var fdsm = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IFibonacciDistributedSequencesManager)) as IFibonacciDistributedSequencesManager;
            CreateAndLinkSendBlock(fdsm, ot);

            fdsm.Init();
        }

        static void CreateAndLinkSendBlock(IFibonacciDistributedSequencesManager seqManager, IOutboundTransport transport)
        {
            seqManager.OutPort.LinkTo(new ActionBlock<FibonacciDistributedSequencesManager.UpdateMessage>(um =>
            {
                transport.Send(new UpdateMessageDto { SequenceId = um.SequenceId, State = um.State.Value });
            }));
        }
    }


    class OutboundHttpTransport : IOutboundTransport
    {
        public HttpClient HttpClient { get; private set; }

        public OutboundHttpTransport(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public Task Send(UpdateMessageDto um)
        {
            return HttpClient.PostAsJsonAsync("api/sequences", um);
        }
    }
}
