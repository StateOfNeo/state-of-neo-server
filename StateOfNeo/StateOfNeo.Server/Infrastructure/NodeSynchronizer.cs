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
        private readonly IOptions<NetSettings> _netsettings;
        private List<Node> CachedDbNodes;

        public NodeSynchronizer(NodeCache nodeCache,
            StateOfNeoContext ctx,
            RPCNodeCaller rPCNodeCaller,
            IOptions<NetSettings> netsettings)
        {
            _nodeCache = nodeCache;
            _ctx = ctx;
            _rPCNodeCaller = rPCNodeCaller;
            _netsettings = netsettings;
            CachedDbNodes = _ctx.Nodes
                .Include(n => n.NodeAddresses)
                .Where(n => n.Net.ToLower() == _netsettings.Value.Net.ToLower())
                .ToList();
        }

        public async Task Init()
        {
            await SyncCacheAndDb();
            await UpdateNodesInformation();
            _nodeCache.Update(_ctx.Nodes.Include(n => n.NodeAddresses).ProjectTo<NodeViewModel>());
        }

        private async Task SyncCacheAndDb()
        {
            foreach (var cacheNode in _nodeCache.NodeList)
            {
                var existingDbNode = CachedDbNodes
                    .FirstOrDefault(dbn =>
                        dbn.NodeAddresses.FirstOrDefault(ia => ia.Ip == cacheNode.Ip) != null);
                if (existingDbNode == null)
                {
                    var newDbNode = Mapper.Map<Node>(cacheNode);
                    newDbNode.Net = _netsettings.Value.Net;
                    _ctx.Nodes.Add(newDbNode);
                    await _ctx.SaveChangesAsync();

                    var nodeDbAddress = new NodeAddress
                    {
                        Ip = cacheNode.Ip,
                        Port = cacheNode.Port,
                        Type = cacheNode.Type == null ? NodeAddressType.Default : Enum.Parse<NodeAddressType>(cacheNode.Type),

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
                            Type = cacheNode.Type == null ? NodeAddressType.Default : Enum.Parse<NodeAddressType>(cacheNode.Type),

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
                    //.Where(n => n.Id == 15)
                    .ToList();

            foreach (var dbNode in dbNodes)
            {
                if (dbNode.Type != NodeAddressType.REST)
                {
                    var newHeight = await _rPCNodeCaller.GetNodeHeight(dbNode);
                    var newVersion = await _rPCNodeCaller.GetNodeVersion(dbNode);

                    if (!string.IsNullOrEmpty(dbNode.SuccessUrl))
                    {
                        var something = "";
                    }

                    dbNode.Version = newVersion;
                    dbNode.Height = newHeight;
                    if (string.IsNullOrEmpty(dbNode.Net))
                    {
                        dbNode.Net = _netsettings.Value.Net;
                    }
                    _ctx.Nodes.Update(dbNode);
                    await _ctx.SaveChangesAsync();

                    //if (string.IsNullOrEmpty(dbNode.Version) ||
                    //    dbNode.Type == NodeAddressType.RPC ||
                    //    string.IsNullOrEmpty(dbNode.Protocol) ||
                    //    dbNode.NodeAddresses.FirstOrDefault(na => na.Type == NodeAddressType.Default) != null)
                    //{
                    //    foreach (var address in dbNode.NodeAddresses)
                    //    {
                    //        if (address.Type == NodeAddressType.Default || !address.Port.HasValue)
                    //        {
                    //            var height = await _rPCNodeCaller.GetNodeHeight(address.Ip, address.Port);
                    //            if (height > -1)
                    //            {
                    //                address.Type = NodeAddressType.RPC;
                    //                dbNode.Type = NodeAddressType.RPC;
                    //                dbNode.Height = height;
                    //            }
                    //        }
                    //        if (string.IsNullOrEmpty(dbNode.Version))
                    //        {
                    //            if (!string.IsNullOrEmpty(dbNode.Protocol))
                    //            {
                    //                var version = await _rPCNodeCaller.GetNodeVersion($"{dbNode.Protocol}://{address.Ip}:{address.Port}");
                    //                if (!string.IsNullOrEmpty(version))
                    //                {
                    //                    dbNode.Version = version;
                    //                }
                    //            }
                    //            else
                    //            {
                    //                foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                    //                {
                    //                    var version = await _rPCNodeCaller.GetNodeVersion($"{protocol}://{address.Ip}:{address.Port}");
                    //                    if (!string.IsNullOrEmpty(version))
                    //                    {
                    //                        dbNode.Version = version;
                    //                        break;
                    //                    }
                    //                }
                    //            }
                    //        }

                    //        _ctx.NodeAddresses.Update(address);
                    //        await _ctx.SaveChangesAsync();
                    //    }

                    //    _ctx.Nodes.Update(dbNode);
                    //    await _ctx.SaveChangesAsync();
                    //}

                }
            }
        }
    }
}
