using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public interface IFailedP2PHub
    {
        Task Send();
    }

    public class FailedP2PHub : Hub<IFailedP2PHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
