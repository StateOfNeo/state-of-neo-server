using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Hubs
{
    public interface ITransactionAverageCountHub
    {
        Task Send();
    }

    public class TransactionAverageCountHub : Hub<ITransactionAverageCountHub>
    {
        public async Task Send() => await Clients.All.Send();
    }
}
