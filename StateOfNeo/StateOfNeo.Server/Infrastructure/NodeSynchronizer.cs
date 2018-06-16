using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Server.Cache;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class NodeSynchronizer
    {
        private NodeCache _nodeCache;
        private StateOfNeoContext _ctx;
        private RPCNodeCaller _rPCNodeCaller;
        private LocationCaller _locationCaller;
        private readonly IOptions<NetSettings> _netsettings;
        public List<Node> CachedDbNodes;

        public NodeSynchronizer(NodeCache nodeCache,
            StateOfNeoContext ctx,
            RPCNodeCaller rPCNodeCaller,
            LocationCaller locationCaller,
            IOptions<NetSettings> netsettings)
        {
            _nodeCache = nodeCache;
            _ctx = ctx;
            _rPCNodeCaller = rPCNodeCaller;
            _locationCaller = locationCaller;
            _netsettings = netsettings;
            UpdateDbCache();
        }

        public IEnumerable<T> GetCachedNodesAs<T>()
        {
            return CachedDbNodes.AsQueryable().ProjectTo<T>();
        }

        private void UpdateDbCache()
        {
            CachedDbNodes = _ctx.Nodes
                .Include(n => n.NodeAddresses)
                .Where(n => n.Net.ToLower() == _netsettings.Value.Net.ToLower())
                .ToList();
        }

        public async Task Init()
        {
            await SyncCacheAndDb();
            await UpdateNodesInformation();
            _nodeCache.Update(CachedDbNodes.AsQueryable().ProjectTo<NodeViewModel>());
        }

        private async Task SyncCacheAndDb()
        {
            foreach (var cacheNode in _nodeCache.NodeList)
            {
                var existingDbNode = CachedDbNodes
                    .FirstOrDefault(dbn => dbn.NodeAddresses.Any(ia => ia.Ip == cacheNode.Ip));

                if (existingDbNode == null)
                {
                    var newDbNode = Mapper.Map<Node>(cacheNode);
                    newDbNode.Type = NodeAddressType.P2P_TCP;
                    newDbNode.Net = _netsettings.Value.Net;
                    _ctx.Nodes.Add(newDbNode);
                    await _ctx.SaveChangesAsync();

                    var nodeDbAddress = new NodeAddress
                    {
                        Ip = cacheNode.Ip,
                        Port = cacheNode.Port,
                        Type = NodeAddressType.P2P_TCP,

                        NodeId = newDbNode.Id
                    };
                    _ctx.NodeAddresses.Add(nodeDbAddress);
                    await _ctx.SaveChangesAsync();
                }
                else
                {
                    var portIsDifferent = existingDbNode.NodeAddresses.FirstOrDefault(na => na.Port == cacheNode.Port) == null;
                    if (portIsDifferent)
                    {
                        var nodeDbAddress = new NodeAddress
                        {
                            Ip = cacheNode.Ip,
                            Port = cacheNode.Port,
                            Type = NodeAddressType.P2P_TCP,

                            NodeId = existingDbNode.Id
                        };
                        _ctx.NodeAddresses.Add(nodeDbAddress);
                        await _ctx.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task UpdateNodesInformation()
        {
            var dbNodes = _ctx.Nodes
                    .Include(n => n.NodeAddresses)
                    .Where(n => n.Net.ToLower() == _netsettings.Value.Net.ToLower())
                    .Where(n => n.Id == 138 || n.Id == 199)
                    .ToList();

            foreach (var dbNode in dbNodes)
            {
                if (dbNode.Type != NodeAddressType.REST)
                {
                    var oldSuccessUrl = dbNode.SuccessUrl;
                    var newHeight = await _rPCNodeCaller.GetNodeHeight(dbNode);
                    if (newHeight != null)
                    {
                        dbNode.Type = NodeAddressType.RPC;
                        dbNode.Height = newHeight;

                        var newVersion = await _rPCNodeCaller.GetNodeVersion(dbNode);
                        dbNode.Version = newVersion;

                        await _locationCaller.UpdateNodeLocation(dbNode.Id);
                        //var peers = await _rPCNodeCaller.GetNodePeers(dbNode);
                        //if (peers != null)
                        //{
                        //    dbNode.Peers = peers.Connected.Count();
                        //}
                        
                        if (string.IsNullOrEmpty(dbNode.Net))
                        {
                            dbNode.Net = _netsettings.Value.Net;
                        }
                        _ctx.Nodes.Update(dbNode);
                        _ctx.SaveChanges();
                    }
                }
            }

            UpdateDbCache();
        }
    }
}
