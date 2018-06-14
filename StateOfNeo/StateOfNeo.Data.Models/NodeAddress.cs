using StateOfNeo.Common;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class NodeAddress
    {
        [Key]
        public int Id { get; set; }
        public uint? Port { get; set; }
        public string Ip { get; set; }
        public NodeAddressType Type { get; set; }

        public int NodeId { get; set; }
        public virtual Node Node { get; set; }
    }
}
