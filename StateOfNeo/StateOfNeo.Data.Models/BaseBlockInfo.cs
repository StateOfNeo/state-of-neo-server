namespace StateOfNeo.Data.Models
{
    public class BaseBlockInfo
    {
        public int SecondsCount { get; set; }
        public long TxCount { get; set; }
        public long TxSystemFees { get; set; }
        public long TxNetworkFees { get; set; }
        public long TxOutputValues { get; set; }
        public ulong BlockHeight { get; set; }
    }
}
