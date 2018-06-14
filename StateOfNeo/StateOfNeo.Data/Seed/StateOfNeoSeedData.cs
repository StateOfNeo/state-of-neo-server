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

        public StateOfNeoSeedData(StateOfNeoContext ctx)
        {
            _ctx = ctx;
        }

        public void Init()
        {
            SeedNodes();
        }

        private void SeedNodes()
        {
            if (!_ctx.Nodes.Any())
            {
                SeedNodesByNetType(NetConstants.MAIN_NET);
                SeedNodesByNetType(NetConstants.TEST_NET);
                //var mainNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText(@"seed-mainnet.json"))).ToObject<List<Node>>();
                //foreach (var node in mainNodes)
                //{

                //    node.Id = 0;
                //    node.Net = NetConstants.MAIN_NET;
                //    _ctx.Nodes.Add(node);
                //    _ctx.SaveChanges();
                //}

                //var testNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText(@"seed-testnet.json"))).ToObject<List<Node>>();
                //foreach (var node in testNodes)
                //{
                //    node.Id = 0;
                //    node.Net = NetConstants.TEST_NET;
                //    _ctx.Nodes.Add(node);
                //    _ctx.SaveChanges();
                //}
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
                    Type = node.Type,
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
