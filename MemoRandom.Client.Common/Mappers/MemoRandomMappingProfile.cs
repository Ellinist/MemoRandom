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

            CreateMap<DbCategory, Category>();

            CreateMap<DbComparedHuman, ComparedHuman>().ReverseMap();

            CreateMap<DbHuman, Human>().ReverseMap();
        }
    }
}
