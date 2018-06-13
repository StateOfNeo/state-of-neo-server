using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StateOfNeo.Server.Hubs;
using StateOfNeo.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Controllers
{
    public class NodeController : BaseApiController
    {
        private readonly IHubContext<NodeHub> _nodeHub;

        public NodeController(IHubContext<NodeHub> nodeHub)
        {
            _nodeHub = nodeHub;
        }

        [HttpPost]
        public async Task Post()
        {
            var nodes = NodeEngine.GetNodesByBFSAlgo();
            await _nodeHub.Clients.All.SendAsync("Receive", nodes);
        }
    }
}
