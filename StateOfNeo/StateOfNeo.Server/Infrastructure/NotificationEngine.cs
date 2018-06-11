using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using Neo.Network;
using StateOfNeo.Server.Hubs;
using StateOfNeo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private readonly IHubContext<BlockHub> _blockHub;
        private List<NodeViewModel> AllNodes = new List<NodeViewModel>();

        public NotificationEngine(IHubContext<BlockHub> blockHub)
        {
            _blockHub = blockHub;
        }

        public async void Init()
        {
            //LocalNode.InventoryReceived += LocalNode_InventoryReceived;
            //LocalNode.InventoryReceiving += LocalNode_InventoryReceiving;
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private void LocalNode_InventoryReceiving(object sender, InventoryReceivingEventArgs e)
        {
            var badpeers = Startup.localNode.GetBadPeers();
            var nodes = Startup.localNode.GetRemoteNodes();
            var unconnected = Startup.localNode.GetUnconnectedPeers();
            foreach (var node in nodes)
            {
                BFSNodes(node);
                //var existingNode = AllNodes
                //    .FirstOrDefault(
                //        n => n.IPAddress == node.RemoteEndpoint.Address.ToString() &&
                //        n.Port == node.RemoteEndpoint.Port);

                //if (existingNode == null)
                //{
                //    AllNodes.Add(new NodeViewModel
                //    {
                //        IPAddress = node.RemoteEndpoint.Address.ToString(),
                //        Port = node.RemoteEndpoint.Port,
                //        Version = node.Version.UserAgent
                //    });
                //}
            }
        }

        private void BFSNodes(RemoteNode node)
        {
            //var newNode = new NodeViewModel();
            //var existingNode = AllNodes
            //        .FirstOrDefault(
            //            n => n.IPAddress == node.RemoteEndpoint.Address.ToString() &&
            //            n.Port == node.RemoteEndpoint.Port &&
            //            !n.IsParent);

            //if (existingNode != null)
            //{
            //    return;
            //}
            //var nodes = node.localNode.GetRemoteNodes();
            //if (nodes.Count() > 0)
            //{
            //    newNode = new NodeViewModel
            //    {
            //        IPAddress = node.RemoteEndpoint.Address.ToString(),
            //        Port = node.RemoteEndpoint.Port,
            //        Version = node.Version.UserAgent,
            //        IsParent = true
            //    };
            //    AllNodes.Add(newNode);
            //}
            //foreach (var remoteNode in nodes)
            //{
            //    BFSNodes(remoteNode);
            //}
            //return;
        }

        private void LocalNode_InventoryReceived(object sender, IInventory e)
        {
            //await _blockHub.Clients.All.SendAsync("Receive", "[SYSTEM] : ", $"{DateTime.Now.ToString("hh:ss")} -> ${Blockchain.Default.Height}");
            //throw new NotImplementedException();
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            //var badpeers = Startup.localNode.GetBadPeers();
            //var nodes = Startup.localNode.GetRemoteNodes();
            //var unconnected = Startup.localNode.GetUnconnectedPeers();
            //foreach (var node in nodes)
            //{
            //    BFSNodes(node);
            //}
            await _blockHub.Clients.All.SendAsync("Receive", $"{DateTime.Now.ToString("hh:ss")} -> ${Blockchain.Default.Height}");
        }
    }
}
