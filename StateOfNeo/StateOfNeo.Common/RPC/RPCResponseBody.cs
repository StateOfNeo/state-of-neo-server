using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCResponseBody : RPCBaseBody
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }
    }
}
