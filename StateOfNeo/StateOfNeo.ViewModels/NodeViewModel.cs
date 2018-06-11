namespace StateOfNeo.ViewModels
{
    public class NodeViewModel
    {
        public string Version { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public string Dns { get; set; }
        public bool IsParent { get; set; }
    }
}
