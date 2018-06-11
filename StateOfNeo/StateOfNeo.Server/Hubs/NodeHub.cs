using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public interface INoteHub
    {
        Task Send();
    }

    public class NodeHub : Hub<INoteHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
