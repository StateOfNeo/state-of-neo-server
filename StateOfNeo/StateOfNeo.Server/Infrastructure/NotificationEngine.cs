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
using Neo;
using Neo.Implementations.Blockchains.LevelDB;
using AutoMapper.QueryableExtensions;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private int NeoBlocksWithoutNodesUpdate = 0;
        private ulong TotalTransactionCount = 0;
        private DateTime LastBlockReceiveTime;
        private readonly StateOfNeoContext _ctx;
        private readonly IHubContext<NodeHub> _nodeHub;
        private readonly IHubContext<BlockHub> blockHub;
        private readonly NodeCache _nodeCache;
        private readonly IHubContext<TransactionCountHub> _transCountHub;
        private readonly IHubContext<TransactionAverageCountHub> _transAvgCountHub;
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly RPCNodeCaller _rPCNodeCaller;
        private readonly NetSettings _netSettings;

        public NotificationEngine(IHubContext<NodeHub> nodeHub,
            IHubContext<BlockHub> blockHub,
            IHubContext<TransactionCountHub> transCountHub,
            IHubContext<TransactionAverageCountHub> transAvgCountHub,
            NodeCache nodeCache,
            NodeSynchronizer nodeSynchronizer,
            RPCNodeCaller rPCNodeCaller,
            StateOfNeoContext ctx,
            IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _nodeHub = nodeHub;
            _nodeCache = nodeCache;
            _transCountHub = transCountHub;
            _transAvgCountHub = transAvgCountHub;
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
            //if (TotalTransactionCount == 0)
            //{
            //    GetTotalTransactionCount(e, ref TotalTransactionCount);
            //}
            //else
            //{
            //    TotalTransactionCount += (ulong)e.Transactions.Length;
            //}
            //await _transCountHub.Clients.All.SendAsync("Receive", TotalTransactionCount);
            //await _transAvgCountHub.Clients.All.SendAsync("Receive", (double)TotalTransactionCount / (double)e.Header.Index);

            await this.blockHub.Clients.All.SendAsync("Receive", e.Header.Index);
            ulong secondsElapsed = 20;
            if (LastBlockReceiveTime != default(DateTime))
            {
                secondsElapsed = (ulong)(DateTime.UtcNow - LastBlockReceiveTime).TotalSeconds;
            }

            if (NotificationConstants.DEFAULT_NEO_BLOCKS_STEP < NeoBlocksWithoutNodesUpdate)
            {
                NeoBlocksWithoutNodesUpdate = 0;
                var nodes = this._nodeSynchronizer.GetCachedNodesAs<NodeViewModel>();
                await _nodeHub.Clients.All.SendAsync("Receive", nodes);
                this._nodeCache.Update(NodeEngine.GetNodesByBFSAlgo());
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

        private void GetTotalTransactionCount(Block block, ref ulong totalTransactions)
        {
            var height = Blockchain.Default.Height;
            var firstBlock = height < 50000 ? 0 : height - 50000;
            for (uint i = firstBlock; i < height; i++)
            {
                var hBlock = Startup.BlockChain.GetBlock(height - i);
                totalTransactions += (uint)hBlock.Transactions.Length;
            }
        }
    }
}
