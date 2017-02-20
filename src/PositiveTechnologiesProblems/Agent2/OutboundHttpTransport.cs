using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PositiveTechnologiesProblems.Agent2
{
    class OutboundHttpTransport : IOutboundTransport
    {
        private readonly HttpClient _httpClient;

        public string BaseAddress { get; private set; }

        public OutboundHttpTransport(string baseAddress)
        {
            BaseAddress = baseAddress;

            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task Send(UpdateMessageDto um)
        {
            return _httpClient.PostAsJsonAsync("api/sequences", um);
        }
    }
}