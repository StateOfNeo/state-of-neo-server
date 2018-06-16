using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neo;
using Neo.Core;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using StateOfNeo.Data.Seed;
using StateOfNeo.Server.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Server.Controllers
{
    public class ValuesController : BaseApiController
    {
        private readonly StateOfNeoContext _ctx;
        private readonly NodeSynchronizer _nodeSynchronizer;
        private readonly LocationCaller _locationCaller;
        private readonly NetSettings _netSettings;

        public ValuesController(NodeSynchronizer nodeSynchronizer, LocationCaller locationCaller, StateOfNeoContext ctx, IOptions<NetSettings> netSettings)
        {
            _ctx = ctx;
            _nodeSynchronizer = nodeSynchronizer;
            _locationCaller = locationCaller;
            _netSettings = netSettings.Value;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", Blockchain.Default.Height.ToString() };
            //return new string[] { "value1" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post()
        {
            //_locationCaller.UpdateAllNodeLocations().ConfigureAwait(false);
            _nodeSynchronizer.Init().ConfigureAwait(false);
            UpdateBlockInfoDB(200865);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
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
                        SecondsCount = 20,
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
                        SecondsCount = 20,
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
    }
}
