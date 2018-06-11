using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private int NeoBlocksWithoutNodesUpdate = 0;
        private readonly IHubContext<NodeHub> _nodeHub;
        private List<NodeViewModel> AllNodes = new List<NodeViewModel>();

        public NotificationEngine(IHubContext<NodeHub> nodeHub)
        {
            _nodeHub = nodeHub;
        }

        public void Init()
        {
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            if (NotificationConstants.DEFAULT_NEO_BLOCKS_STEP == NeoBlocksWithoutNodesUpdate)
            {
                AllNodes.Clear();
                AllNodes = NodeEngine.GetNodesByBFSAlgo();
                NeoBlocksWithoutNodesUpdate = 0;
                await _nodeHub.Clients.All.SendAsync("Receive", AllNodes);
            }

            NeoBlocksWithoutNodesUpdate++;
        }
    }
}
