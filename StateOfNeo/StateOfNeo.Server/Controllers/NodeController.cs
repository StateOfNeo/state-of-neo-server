using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class NodeController : BaseApiController
    {
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly NodeCache _nodeCache;

        public NodeController(IHubContext<NodeHub> nodeHub, NodeCache nodeCache)
        {
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
        }

        [HttpPost]
        public async Task Post()
        {
            await _nodeHub.Clients.All.SendAsync("Receive", _nodeCache.NodeList);
        }
    }
}
