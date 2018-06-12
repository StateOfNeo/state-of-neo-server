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
                        n => n.IPAddress == node.RemoteEndpoint.Address.ToString() &&
                        n.Port == node.RemoteEndpoint.Port);

            if (existingNode != null)
            {
                return;
            }

            var privateNode =  ObjectExtensions.GetInstanceField<LocalNode>(typeof(RemoteNode), node, "localNode");
            
            var nodes = privateNode.GetRemoteNodes();
            newNode = new NodeViewModel
            {
                IPAddress = node.RemoteEndpoint.Address.ToString(),
                Port = node.RemoteEndpoint.Port,
                Version = node.Version?.UserAgent
            };
            nodeViewModels.Add(newNode);
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
