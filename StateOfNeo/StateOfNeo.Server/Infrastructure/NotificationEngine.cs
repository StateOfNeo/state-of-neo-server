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
using StateOfNeo.Data.Models;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private int NeoBlocksWithoutNodesUpdate = 0;
        private ulong TotalTransactionCount = 0;
        private DateTime LastBlockReceiveTime = default(DateTime);
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
            //UpdateBlockInfoDB(e.Header.Index);
            var blockInfo = UpdateBlockInfo(e);
            var totalTransactionsCount = GetTotalTransactionCount();
            await _transCountHub.Clients.All.SendAsync("Receive", totalTransactionsCount);
            var avgTransCount = GetAverageTransactionCount();
            await _transAvgCountHub.Clients.All.SendAsync("Receive", avgTransCount);

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

        public void UpdateBlockInfoDB(uint startHeight)
        {
            for (uint i = startHeight; i > 0; i--)
            {
                var newBLock = Startup.BlockChain.GetBlock(i);
                UpdateBlockInfo(newBLock);
            }
        }

        private BaseBlockInfo UpdateBlockInfo(Block block)
        {
            var txsysfee = (int)block.Transactions.Select(t => t.SystemFee).Sum();
            var txnetfee = (int)Block.CalculateNetFee(block.Transactions);
            var txoutvalues = (int)block.Transactions.Select(t => t.Outputs.Select(o => o.Value)).SelectMany(x => x).Sum();
            BaseBlockInfo newBlockInfo = null;

            if (_netSettings.Net == NetConstants.MAIN_NET)
            {
                var info = _ctx.MainNetBlockInfos.FirstOrDefault(x => x.BlockHeight == block.Header.Index);
                if (info == null)
                {
                    var dbInfo = new MainNetBlockInfo
                    {
                        BlockHeight = block.Header.Index,
                        SecondsCount = LastBlockReceiveTime == default(DateTime) ? 20 : (int)(DateTime.UtcNow - LastBlockReceiveTime).TotalSeconds,
                        TxCount = block.Transactions.Length,
                        TxSystemFees = txsysfee,
                        TxNetworkFees = txnetfee,
                        TxOutputValues = txoutvalues
                    };
                    _ctx.MainNetBlockInfos.Add(dbInfo);
                    _ctx.SaveChanges();
                    newBlockInfo = dbInfo;
                }
            }
            else
            {
                var info = _ctx.TestNetBlockInfos.FirstOrDefault(x => x.BlockHeight == block.Header.Index);
                if (info == null)
                {
                    var dbInfo = new TestNetBlockInfo
                    {
                        BlockHeight = block.Header.Index,
                        SecondsCount = (int)(DateTime.UtcNow - LastBlockReceiveTime).TotalSeconds,
                        TxCount = block.Transactions.Length,
                        TxSystemFees = txsysfee,
                        TxNetworkFees = txnetfee,
                        TxOutputValues = txoutvalues
                    };
                    _ctx.TestNetBlockInfos.Add(dbInfo);
                    _ctx.SaveChanges();
                    newBlockInfo = dbInfo;
                }
            }
            return newBlockInfo;
        }

        private long GetTotalTransactionCount()
        {
            if (_netSettings.Net == NetConstants.MAIN_NET)
            {
                return _ctx.MainNetBlockInfos.Sum(x => x.TxCount);
            }
            return _ctx.TestNetBlockInfos.Sum(x => x.TxCount);
        }

        private decimal GetAverageTransactionCount()
        {
            if (_netSettings.Net == NetConstants.MAIN_NET)
            {
                return (decimal)GetTotalTransactionCount() / (decimal)_ctx.MainNetBlockInfos.OrderByDescending(x => x.BlockHeight).First().BlockHeight;
            }
            return (decimal)GetTotalTransactionCount() / (decimal)_ctx.TestNetBlockInfos.OrderByDescending(x => x.BlockHeight).First().BlockHeight;
        }
    }
}
