using Newtonsoft.Json;
using StateOfNeo.Common.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class RPCNodeCaller
    {
        public RPCNodeCaller()
        {

        }

        public async Task<int> GetNodeHeight(string endpoint)
        {
            var rpcRequest = new RPCRequestBody();
            var response = await SendRPCCall(HttpMethod.Post, endpoint, rpcRequest);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializedResult = JsonConvert.DeserializeObject<RPCResponseBody>(result);
                return int.Parse(serializedResult.Result);
            }
            return -1;
        }

        private async Task<HttpResponseMessage> SendRPCCall(HttpMethod httpMethod, string endpoint, object rpcData)
        {
            HttpResponseMessage response;

            using (var http = new HttpClient())
            {
                var req = new HttpRequestMessage(httpMethod, $"{endpoint}");

                var data = JsonConvert.SerializeObject(rpcData, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });

                req.Content = new StringContent(data, Encoding.Default, "application/json");
                response = await http.SendAsync(req);
            }

            return response;
        }
    }
}
