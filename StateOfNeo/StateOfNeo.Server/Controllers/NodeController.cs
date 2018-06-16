using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using StateOfNeo.ViewModels;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class NodeController : BaseApiController
    {
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly NodeCache _nodeCache;
        private readonly NodeSynchronizer nodeSynchronizer;

        public NodeController(IHubContext<NodeHub> nodeHub, NodeCache nodeCache, NodeSynchronizer nodeSynchronizer)
        {
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            this.nodeSynchronizer = nodeSynchronizer;
        }

        [HttpPost]
        public async Task Post()
        {
            await _nodeHub.Clients.All.SendAsync("Receive", this.nodeSynchronizer.GetCachedNodesAs<NodeViewModel>());
        }
    }
}
