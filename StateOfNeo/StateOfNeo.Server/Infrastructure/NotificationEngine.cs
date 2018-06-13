using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Linq;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private int NeoBlocksWithoutNodesUpdate = 0;
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly IHubContext<BlockHub> blockHub;
        private readonly NodeCache _nodeCache;

        public NotificationEngine(IHubContext<NodeHub> nodeHub, 
            IHubContext<BlockHub> blockHub,
            NodeCache nodeCache)
        {
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            this.blockHub = blockHub;
        }

        public void Init()
        {
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            await this.blockHub.Clients.All.SendAsync("Receive", e.Header.Index);

            if (NotificationConstants.DEFAULT_NEO_BLOCKS_STEP < NeoBlocksWithoutNodesUpdate)
            {
                _nodeCache.NodeList.Clear();
                _nodeCache.Update(NodeEngine.GetNodesByBFSAlgo());
                NeoBlocksWithoutNodesUpdate = 0;
                await _nodeHub.Clients.All.SendAsync("Receive", _nodeCache.NodeList);
            }

            NeoBlocksWithoutNodesUpdate++;
        }
    }
}
