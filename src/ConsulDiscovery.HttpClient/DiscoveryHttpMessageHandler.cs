using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ConsulDiscovery.HttpClient
{
    public class DiscoveryHttpMessageHandler : DelegatingHandler
    {
        private static readonly Random random = new Random((int)DateTime.Now.Ticks);

        private readonly DiscoveryClient discoveryClient;

        public DiscoveryHttpMessageHandler(DiscoveryClient discoveryClient)
        {
            this.discoveryClient = discoveryClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (discoveryClient.AllServices.TryGetValue(request.RequestUri.Host, out var serviceHosts))
            {
                if (serviceHosts.Count > 0)
                {
                    var index = random.Next(serviceHosts.Count);
                    request.RequestUri = new Uri(new Uri(serviceHosts[index]), request.RequestUri.PathAndQuery);
                }
            }
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
