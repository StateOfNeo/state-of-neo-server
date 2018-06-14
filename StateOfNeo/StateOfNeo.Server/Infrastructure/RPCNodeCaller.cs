using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Common.RPC;
using StateOfNeo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class RPCNodeCaller
    {
        public RPCNodeCaller()
        {

        }

        public async Task<int> GetNodeHeight(Node node)
        {
            var result = -1;
            foreach (var defaultPort in RPCCallConstants.PORTS_TESTS)
            {
                bool isSucessful = false;
                foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                {
                    var httpResult = await MakeRPCCall<RPCResponseBody<int>>(node);
                    if (httpResult.Result > 0)
                    {
                        result = httpResult.Result;
                        isSucessful = true;
                        break;
                    }
                }
                if (isSucessful)
                {
                    break;
                }
            }
            return result;
        }

        public async Task<int> GetNodeHeight(string ip, uint? port)
        {
            var result = -1;

            if (port == null)
            {
                foreach (var defaultPort in RPCCallConstants.PORTS_TESTS)
                {
                    bool isSucessful = false;
                    foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                    {
                        var httpResult = await MakeRPCCall<RPCResponseBody<int>>($"{protocol}://{ip}:{defaultPort.ToString()}");
                        //var httpResult = await MakeRPCCall<RPCResponseBody<int>>(node);
                        if (httpResult.Result > 0)
                        {
                            result = httpResult.Result;
                            isSucessful = true;
                            break;
                        }
                    }
                    if (isSucessful)
                    {
                        break;
                    }
                }
            }
            else
            {
                var httpResult = await MakeRPCCall<RPCResponseBody<int>>($"{ip}:{port}");
                if (httpResult.Result > 0)
                {
                    result = httpResult.Result;
                }
            }
            return result;
        }

        public async Task<string> GetNodeVersion(string endpoint)
        {
            var result = await MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(endpoint, "getversion");
            if (result != null)
            {
                return result.Result.Useragent;
            }
            return string.Empty;
        }

        public async Task<string> GetNodeVersion(Node node)
        {
            if (!string.IsNullOrEmpty(node.Version))
            {
                var result = await MakeRPCCall<RPCResponseBody<RPCResultGetVersion>>(node, "getversion");
                if (result != null)
                {
                    return result.Result.Useragent;
                }
            }
            return string.Empty;
        }

        private async Task<T> MakeRPCCall<T>(Node node, string method = "getblockcount")
        {
            HttpResponseMessage response = null;
            bool succesfulCall = false;
            var successUrl = string.Empty;
            var url = string.Empty;

            var rpcRequest = new RPCRequestBody
            {
                Method = method
            };

            if (!string.IsNullOrEmpty(node.SuccessUrl))
            {
                response = await SendRPCCall(HttpMethod.Post, $"{node.SuccessUrl}", rpcRequest);
                if (response.IsSuccessStatusCode)
                {
                    succesfulCall = true;
                }
            }
            else if (!string.IsNullOrEmpty(node.Protocol) &&
                 !string.IsNullOrEmpty(node.Url))
            {
                foreach (var address in node.NodeAddresses)
                {
                    if (address.Port.HasValue)
                    {
                        url = $"{node.Protocol}://{node.Url}:{address.Port}";
                        response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                        if (response.IsSuccessStatusCode)
                        {
                            node.Type = NodeAddressType.RPC;
                            address.Type = NodeAddressType.RPC;
                            successUrl = url;
                            succesfulCall = true;
                            break;
                        }
                    }
                }
                if (!succesfulCall)
                {
                    foreach (var port in RPCCallConstants.PORTS_TESTS)
                    {
                        url = $"{node.Protocol}://{node.Url}:{port}";
                        response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                        if (response.IsSuccessStatusCode)
                        {
                            node.Type = NodeAddressType.RPC;
                            successUrl = url;
                            break;
                        }
                    }
                }
            }
            else if (string.IsNullOrEmpty(node.Protocol) &&
                !string.IsNullOrEmpty(node.Url))
            {
                if (node.Url.Contains(RPCCallConstants.PROTOCOL_TYPES_TESTS[0]) ||
                    node.Url.Contains(RPCCallConstants.PROTOCOL_TYPES_TESTS[1]))
                {
                    foreach (var address in node.NodeAddresses)
                    {
                        if (address.Port.HasValue)
                        {
                            url = $"{node.Url}:{address.Port}";
                            response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                            if (response.IsSuccessStatusCode)
                            {
                                node.Type = NodeAddressType.RPC;
                                address.Type = NodeAddressType.RPC;
                                successUrl = url;
                                succesfulCall = true;
                                break;
                            }
                        }
                    }
                    if (!succesfulCall)
                    {
                        foreach (var port in RPCCallConstants.PORTS_TESTS)
                        {
                            url = $"{node.Url}:{port}";
                            response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                            if (response.IsSuccessStatusCode)
                            {
                                node.Type = NodeAddressType.RPC;
                                successUrl = url;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var address in node.NodeAddresses)
                    {
                        if (address.Port.HasValue)
                        {
                            foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                            {
                                url = $"{protocol}://{node.Url}:{address.Port}";
                                response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    address.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    succesfulCall = true;
                                    break;
                                }
                            }
                        }
                        if (succesfulCall) break;
                    }
                    if (!succesfulCall)
                    {
                        foreach (var port in RPCCallConstants.PORTS_TESTS)
                        {
                            foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                            {
                                url = $"{protocol}://{node.Url}:{port}";
                                response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    succesfulCall = true;
                                    break;
                                }
                            }
                            if (succesfulCall)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var address in node.NodeAddresses)
                {
                    if (address.Port.HasValue)
                    {
                        foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                        {
                            url = $"{protocol}://{address.Ip}:{address.Port}";
                            response = await SendRPCCall(HttpMethod.Post, $"{protocol}://{address.Ip}:{address.Port}", rpcRequest);
                            if (response.IsSuccessStatusCode)
                            {
                                node.Type = NodeAddressType.RPC;
                                address.Type = NodeAddressType.RPC;
                                successUrl = url;
                                succesfulCall = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (var port in RPCCallConstants.PORTS_TESTS)
                        {
                            foreach (var protocol in RPCCallConstants.PROTOCOL_TYPES_TESTS)
                            {
                                url = $"{protocol}://{address.Ip}:{port}";
                                response = await SendRPCCall(HttpMethod.Post, url, rpcRequest);
                                if (response.IsSuccessStatusCode)
                                {
                                    node.Type = NodeAddressType.RPC;
                                    address.Type = NodeAddressType.RPC;
                                    successUrl = url;
                                    succesfulCall = true;
                                    break;
                                }
                            }
                            if (succesfulCall)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(node.SuccessUrl))
            {
                node.SuccessUrl = successUrl;
            }

            if (succesfulCall)
            {
                if (response != null && response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serializedResult = JsonConvert.DeserializeObject<T>(result);
                    return serializedResult;
                }
            }
            return default(T);
        }

        private async Task<T> MakeRPCCall<T>(string endpoint, string method = "getblockcount")
        {
            var rpcRequest = new RPCRequestBody
            {
                Method = method
            };
            var response = await SendRPCCall(HttpMethod.Post, endpoint, rpcRequest);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var serializedResult = JsonConvert.DeserializeObject<T>(result);
                return serializedResult;
            }
            return default(T);
        }

        private async Task<HttpResponseMessage> SendRPCCall(HttpMethod httpMethod, string endpoint, object rpcData)
        {
            HttpResponseMessage response;
            try
            {
                var httpHandler = new HttpClientHandler();
                httpHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                httpHandler.SslProtocols = SslProtocols.Tls12;
                httpHandler.ClientCertificates.Add(new X509Certificate());
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
            }
            catch (Exception e)
            {
                return null;
            }

            return response;
        }
    }
}
