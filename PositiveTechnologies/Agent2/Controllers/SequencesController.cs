using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks.Dataflow;
using System.Web.Http;
using log4net;
using PositiveTechnologies;

namespace Agent2.Controllers
{
    public class SequencesController : ApiController
    {
        private readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IFibonacciDistributedSequencesManager SequencesManager { get; private set; }

        public SequencesController(IFibonacciDistributedSequencesManager sequencesManager)
        {
            SequencesManager = sequencesManager;
        }

        public HttpResponseMessage PostSequenceState(UpdateMessageDto um)
        {
            log.Info(String.Format("Incoming state {0} for sequence {1}", um.State, um.SequenceId));
            SequencesManager.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage
            {
                SequenceId = um.SequenceId,
                State = new FibonacciState(um.State)
            });
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
