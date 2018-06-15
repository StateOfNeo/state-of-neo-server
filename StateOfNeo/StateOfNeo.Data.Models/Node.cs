using StateOfNeo.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class Node
    {
        private IEnumerable<NodeAddress> _nodeAddresses;
        private IEnumerable<TimeEvent> _events;

        public Node()
        {
            _nodeAddresses = new HashSet<NodeAddress>();
            _events = new HashSet<TimeEvent>();
        }

        [Key]
        public int Id { get; set; }

        public string Protocol { get; set; }
        public string Url { get; set; }

        public int? Height { get; set; }
        public int? Peers { get; set; }
        public int? MemoryPool { get; set; }
        public string Version { get; set; }
        public NodeAddressType Type { get; set; }
        
        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string FlagUrl { get; set; }

        public string SuccessUrl { get; set; }
        public string Net { get; set; }

        public virtual IEnumerable<NodeAddress> NodeAddresses
        {
            get { return _nodeAddresses; }
            set { _nodeAddresses = value; }
        }

        public virtual IEnumerable<TimeEvent> Events
        {
            get { return _events; }
            set { _events = value; }
        }
    }
}
