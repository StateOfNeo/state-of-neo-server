using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateOfNeo.Common;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Data.Seed
{
    public class StateOfNeoSeedData
    {
        private readonly StateOfNeoContext _ctx;
        private readonly IOptions<NetSettings> _netSettings;

        public StateOfNeoSeedData(StateOfNeoContext ctx, IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _netSettings = netSettings;
        }

        public void Init()
        {
            SeedBlockchainInfo();
            SeedNodes();
        }

        private void SeedBlockchainInfo()
        {
            if (!_ctx.BlockchainInfos.Any())
            {
                var blockinfoTestnet = new BlockchainInfo
                {
                    BlockCount = 1537395,
                    SecondsCount = 109616263,
                    Net = NetConstants.TEST_NET
                };

                var blockinfoMainNet = new BlockchainInfo
                {
                    BlockCount = 2392397,
                    SecondsCount = 44259344,
                    Net = NetConstants.MAIN_NET
                };
                _ctx.BlockchainInfos.Add(blockinfoMainNet);
                _ctx.BlockchainInfos.Add(blockinfoTestnet);
                _ctx.SaveChanges();
            }
        }

        private void SeedNodes()
        {
            if (!_ctx.Nodes.Any())
            {
                SeedNodesByNetType(NetConstants.MAIN_NET);
                SeedNodesByNetType(NetConstants.TEST_NET);
            }
        }

        private void SeedNodesByNetType(string net)
        {
            var mainNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText($@"seed-{net.ToLower()}.json"))).ToObject<List<NodeViewModel>>();
            foreach (var node in mainNodes)
            {
                var newNode = new Node
                {
                    Id = 0,
                    Net = net,
                    Locale = node.Locale,
                    Location = node.Location,
                    Protocol = node.Protocol,
                    Url = node.Url,
                    Type = Enum.Parse<NodeAddressType>(node.Type),
                    Version = node.Version
                };
                _ctx.Nodes.Add(newNode);
                _ctx.SaveChanges();

                RegisterIpAddresses(newNode.Id, node);
            }
        }

        private void RegisterIpAddresses(int nodeId, NodeViewModel node)
        {
            var newAddress = new NodeAddress
            {
                Ip = node.Ip,
                Port = node.Port,
                NodeId = nodeId,
                Type = Enum.Parse<NodeAddressType>(node.Type)
            };
            _ctx.NodeAddresses.Add(newAddress);
            _ctx.SaveChanges();
        }
    }
}
