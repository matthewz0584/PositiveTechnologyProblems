using System.Net;
using System.Net.Http;
using System.Threading.Tasks.Dataflow;
using System.Web.Http;

namespace PositiveTechnologiesProblems.Agent2.Controllers
{
    public class SequencesController : ApiController
    {
        public ITargetBlock<UpdateMessageDto> ChainStart { get; private set; }

        public SequencesController(ITargetBlock<UpdateMessageDto> chainStart)
        {
            ChainStart = chainStart;
        }

        public HttpResponseMessage PostSequenceState(UpdateMessageDto um)
        {
            ChainStart.Post(um);
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}
