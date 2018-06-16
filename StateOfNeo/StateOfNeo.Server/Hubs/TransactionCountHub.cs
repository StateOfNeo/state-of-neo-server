using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public interface ITransactionCountHub
    {
        Task Send();
    }

    public class TransactionCountHub : Hub<ITransactionCountHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
