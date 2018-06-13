using AutoMapper;
using StateOfNeo.Data.Models;
using StateOfNeo.ViewModels;

namespace StateOfNeo.Infrastructure.Mapping
{
    internal class NodeConfig
    {
        public static void InitMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Node, NodeViewModel>().ReverseMap();
        }
    }
}
