using Newtonsoft.Json;

namespace StateOfNeo.Common.RPC
{
    public class RPCResponseBody<T> : RPCBaseBody
    {
        [JsonProperty(PropertyName = "result")]
        public T Result { get; set; }
    }
}
