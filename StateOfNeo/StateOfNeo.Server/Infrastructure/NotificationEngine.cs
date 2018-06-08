using Microsoft.AspNetCore.SignalR;
using Neo.Core;
using StateOfNeo.Server.Hubs;
using System;

namespace StateOfNeo.Server.Infrastructure
{
    public class NotificationEngine
    {
        private readonly IHubContext<BlockHub> _blockHub;

        public NotificationEngine(IHubContext<BlockHub> blockHub)
        {
            _blockHub = blockHub;
        }

        public async void Init()
        {
            Blockchain.PersistCompleted += UpdateBlockCount_Completed;
        }

        private async void UpdateBlockCount_Completed(object sender, Block e)
        {
            await _blockHub.Clients.All.SendAsync("Receive", "[SYSTEM] : ", $"{DateTime.Now.ToString("hh:ss")} -> ${Blockchain.Default.Height}");
        }
    }
}
