using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class BlockchainInfo
    {
        [Key]
        public int Id { get; set; }
        public ulong SecondsCount { get; set; }
        public uint BlockCount { get; set; }
        public string Net { get; set; }

        public decimal AverageBlockTime
        {
            get
            {
                var result = (decimal)SecondsCount / (decimal)BlockCount;
                return result;
            }
        }
    }
}
