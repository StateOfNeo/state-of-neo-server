using Neo.Network;
using StateOfNeo.ViewModels;
using StateOfNeo.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StateOfNeo.Server.Infrastructure
{
    public static class NodeEngine
    {
        private static void BFSNodes(RemoteNode node, ref List<NodeViewModel> nodeViewModels)
        {
            var newNode = new NodeViewModel();
            var existingNode = nodeViewModels
                    .FirstOrDefault(
                        n => n.Ip == node.RemoteEndpoint.Address.ToString().ToMatchedIp() &&
                        n.Port == node.RemoteEndpoint.Port);

            if (existingNode != null)
            {
                return;
            }

            //var privateNode = node.GetFieldValue<LocalNode>("localNode");

            var privateNode = ObjectExtensions.GetInstanceField<LocalNode>(typeof(RemoteNode), node, "localNode");

            var nodes = privateNode.GetRemoteNodes();
            newNode = new NodeViewModel
            {
                Ip = node.RemoteEndpoint.Address.ToString().ToMatchedIp(),
                Port = node.Version?.Port != null ? node.Version.Port : (uint)node.RemoteEndpoint.Port,
                Version = node.Version?.UserAgent,
                RemoteNodesCount = privateNode.RemoteNodeCount,
                UnconectedNodesCount = privateNode.GetUnconnectedPeers().Length
            };
            nodeViewModels.Add(newNode);
            var unconnectedPeers = privateNode.GetUnconnectedPeers();
            foreach (var unconnectedPeer in unconnectedPeers)
            {
                if (nodeViewModels.FirstOrDefault(
                        n => n.Ip == unconnectedPeer.ToString().ToMatchedIp() &&
                        n.Port == unconnectedPeer.Port) == null)
                {
                    var unconnectedNode = new NodeViewModel
                    {
                        Ip = unconnectedPeer.Address.ToString().ToMatchedIp(),
                        Port = (uint)unconnectedPeer.Port
                    };

                    nodeViewModels.Add(unconnectedNode);
                }
            }
            foreach (var remoteNode in nodes)
            {
                BFSNodes(remoteNode, ref nodeViewModels);
            }
            return;
        }

        public static List<NodeViewModel> GetNodesByBFSAlgo()
        {
            var result = new List<NodeViewModel>();
            var badpeers = Startup.localNode.GetBadPeers();
            var nodes = Startup.localNode.GetRemoteNodes();
            var unconnected = Startup.localNode.GetUnconnectedPeers();
            foreach (var node in nodes)
            {
                BFSNodes(node, ref result);
            }
            return result;
        }
    }
}
