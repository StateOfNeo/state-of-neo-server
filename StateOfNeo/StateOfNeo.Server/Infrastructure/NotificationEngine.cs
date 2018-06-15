using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Server.Cache;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Linq;
using System;
using StateOfNeo.Data;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private int NeoBlocksWithoutNodesUpdate = 0;
        private DateTime LastBlockReceiveTime;
        private readonly StateOfNeoContext _ctx;
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly IHubContext<BlockHub> blockHub;
        private readonly NodeCache _nodeCache;
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly RPCNodeCaller _rPCNodeCaller;
        private readonly NetSettings _netSettings;

        public NotificationEngine(IHubContext<NodeHub> nodeHub,
            IHubContext<BlockHub> blockHub,
            NodeCache nodeCache,
            NodeSynchronizer nodeSynchronizer,
            RPCNodeCaller rPCNodeCaller,
            StateOfNeoContext ctx,
            IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            _nodeSynchronizer = nodeSynchronizer;
            _rPCNodeCaller = rPCNodeCaller;
            _netSettings = netSettings.Value;
            this.blockHub = blockHub;
        }

        public void Init()
        {
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            await this.blockHub.Clients.All.SendAsync("Receive", e.Header.Index);
            ulong secondsElapsed = 20;
            if (LastBlockReceiveTime != default(DateTime))
            {
                secondsElapsed = (ulong)(DateTime.UtcNow - LastBlockReceiveTime).TotalSeconds; //DateTime.UtcNow.Subtract(LastBlockReceiveTime).TotalSeconds;
            }
            //var averageSeconds = await GetAverageBlockTime(secondsElapsed);
            //await this.blockHub.Clients.All.SendAsync("Receive", averageSeconds);

            var average = Blockchain.SecondsPerBlock;

            if (NotificationConstants.DEFAULT_NEO_BLOCKS_STEP < NeoBlocksWithoutNodesUpdate)
            {
                _nodeCache.Update(NodeEngine.GetNodesByBFSAlgo());
                //_nodeSynchronizer.Init().ConfigureAwait(false);
                NeoBlocksWithoutNodesUpdate = 0;
                _nodeHub.Clients.All.SendAsync("Receive", _nodeCache.NodeList).ConfigureAwait(false);
            }

            LastBlockReceiveTime = DateTime.UtcNow;
            NeoBlocksWithoutNodesUpdate++;
        }

        private async Task<decimal> GetAverageBlockTime(ulong secondsElapsed)
        {
            var blockInfo = _ctx.BlockchainInfos.First(bi => bi.Net == _netSettings.Net);
            blockInfo.BlockCount++;
            blockInfo.SecondsCount += secondsElapsed;
            _ctx.BlockchainInfos.Update(blockInfo);
            await _ctx.SaveChangesAsync();

            return blockInfo.AverageBlockTime;
        }
    }
}
