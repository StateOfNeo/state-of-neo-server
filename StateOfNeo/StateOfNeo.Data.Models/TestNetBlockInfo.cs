using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class TestNetBlockInfo : BaseBlockInfo
    {
        [Key]
        public int Id { get; set; }
    }
}
