using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class MainNetBlockInfo : BaseBlockInfo
    {
        [Key]
        public int Id { get; set; }
    }
}
