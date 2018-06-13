﻿namespace StateOfNeo.ViewModels
{
    public class NodeViewModel
    {
        public uint? Port { get; set; }
        public string Ip { get; set; }
        public string Protocol { get; set; }
        public string Url { get; set; }

        public string Version { get; set; }
        public string Type { get; set; }

        public int RemoteNodesCount { get; set; }
        public int UnconectedNodesCount { get; set; }

        public string Locale { get; set; }
        public string Location { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }

        public string Net { get; set; }
    }
}
