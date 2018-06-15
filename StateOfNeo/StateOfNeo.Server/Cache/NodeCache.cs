using StateOfNeo.Data;
using StateOfNeo.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace StateOfNeo.Server.Cache
{
    public class NodeCache
    {
        private readonly StateOfNeoContext _ctx;

        public List<NodeViewModel> NodeList { get; private set; }

        public NodeCache(StateOfNeoContext ctx)
        {
            _ctx = ctx;
            NodeList = new List<NodeViewModel>();
        }

        public void Update(IEnumerable<NodeViewModel> nodeViewModels)
        {
            NodeList.AddRange(nodeViewModels.ToList());
        }
    }
}
