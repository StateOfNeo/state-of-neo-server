﻿using Neo.Network;
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
                existingNode.IsVisited = true;
            }

            //var privateNode = node.GetFieldValue<LocalNode>("localNode");
            var privateNode = ObjectExtensions.GetInstanceField<LocalNode>(typeof(RemoteNode), node, "localNode");
            
            if ((existingNode == null || !existingNode.IsVisited) && node.Version != null)
            {
                newNode = new NodeViewModel
                {
                    Ip = node.RemoteEndpoint.Address.ToString().ToMatchedIp(),
                    Port = node.Version?.Port != null ? node.Version.Port : (uint)node.RemoteEndpoint.Port,
                    Version = node.Version?.UserAgent,
                    Peers = privateNode.RemoteNodeCount,
                };
                nodeViewModels.Add(newNode);
                var nodes = privateNode.GetRemoteNodes();
                foreach (var remoteNode in nodes)
                {
                    BFSNodes(remoteNode, ref nodeViewModels);
                }
            }
            return;
        }

        public static List<NodeViewModel> GetNodesByBFSAlgo()
        {
            var result = new List<NodeViewModel>();
            var nodes = Startup.localNode.GetRemoteNodes();
            foreach (var node in nodes)
            {
                BFSNodes(node, ref result);
            }
            return result;
        }
    }
}
