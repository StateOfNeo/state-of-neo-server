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
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly RPCNodeCaller _rPCNodeCaller;

        public NotificationEngine(IHubContext<NodeHub> nodeHub,
            IHubContext<BlockHub> blockHub,
            NodeCache nodeCache,
            NodeSynchronizer nodeSynchronizer,
            RPCNodeCaller rPCNodeCaller)
        {
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            _nodeSynchronizer = nodeSynchronizer;
            _rPCNodeCaller = rPCNodeCaller;
            this.blockHub = blockHub;
        }

        public void Init()
        {
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            this.blockHub.Clients.All.SendAsync("Receive", e.Header.Index).ConfigureAwait(false);

            if (NotificationConstants.DEFAULT_NEO_BLOCKS_STEP < NeoBlocksWithoutNodesUpdate)
            {
                _nodeCache.NodeList.Clear();
                _nodeCache.Update(NodeEngine.GetNodesByBFSAlgo());
                _nodeSynchronizer.Init().ConfigureAwait(false);
                NeoBlocksWithoutNodesUpdate = 0;
                _nodeHub.Clients.All.SendAsync("Receive", _nodeCache.NodeList).ConfigureAwait(false);
            }

            NeoBlocksWithoutNodesUpdate++;
        }
    }
}
