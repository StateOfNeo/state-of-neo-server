using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;
using System.Linq;

namespace StateOfNeo.Infrastructure.Mapping
{
    internal class NodeConfig
    {
        public static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Node, NodeViewModel>()
                .ForMember(x => x.Ip, 
                    y => y.MapFrom(
                        z => string.Join(", ", z.NodeAddresses.Select(na => $"{na.Ip}:{na.Port}"))));
            cfg.CreateMap<NodeViewModel, Node>();
        }
    }
}
