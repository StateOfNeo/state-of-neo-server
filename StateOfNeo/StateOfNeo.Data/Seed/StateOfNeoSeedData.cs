using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateOfNeo.Common;
using StateOfNeo.Data.Models;
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
                var mainNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText(@"..\seed-mainnet.json"))).ToObject<List<Node>>();
                foreach (var node in mainNodes)
                {
                    node.Id = 0;
                    node.Net = NetConstants.MAIN_NET;
                    _ctx.Nodes.Add(node);
                    _ctx.SaveChanges();
                }

                var testNodes = ((JArray)JsonConvert.DeserializeObject(File.ReadAllText(@"..\seed-testnet.json"))).ToObject<List<Node>>();
                foreach (var node in testNodes)
                {
                    node.Id = 0;
                    node.Net = NetConstants.TEST_NET;
                    _ctx.Nodes.Add(node);
                    _ctx.SaveChanges();
                }
            }
        }
    }
}
