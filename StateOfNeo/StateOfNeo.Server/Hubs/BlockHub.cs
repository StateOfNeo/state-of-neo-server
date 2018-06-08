using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public interface IBlockHub
    {
        Task Send();
    }

    public class BlockHub : Hub<IBlockHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
