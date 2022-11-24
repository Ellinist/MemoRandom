using AutoMapper;
using MemoRandom.Client.Common.Models;
using MemoRandom.Data.DbModels;

namespace MemoRandom.Client.Common.Mappers
{
    public class MemoRandomMappingProfile : Profile
    {
        public MemoRandomMappingProfile()
        {
            CreateMap<DbReason, Reason>().ReverseMap();

            //CreateMap<DbCategory, Category>()
            //    .ForMember(x => x.CategoryId, x => x.MapFrom(src => src.CategoryId))
            //    .ForMember(x => x.CategoryName, x => x.MapFrom(src => src.CategoryName))
            //    .ForPath(x => x.CategoryColor.A, x => x.MapFrom(src => src.ColorA))
            //    .ForPath(x => x.CategoryColor.R, x => x.MapFrom(src => src.ColorR))
            //    .ForPath(x => x.CategoryColor.G, x => x.MapFrom(src => src.ColorG))
            //    .ForPath(x => x.CategoryColor.B, x => x.MapFrom(src => src.ColorB));
            CreateMap<DbCategory, Category>();

            CreateMap<DbComparedHuman, ComparedHuman>().ReverseMap();

            CreateMap<DbHuman, Human>().ReverseMap();
        }
    }
}
