using AutoMapper;
using MemoRandom.Client.Common.Models;
using MemoRandom.Data.DbModels;
using MemoRandom.Data.DtoModels;

namespace MemoRandom.Client.Common.Mappers
{
    public class MemoRandomMappingProfile : Profile
    {
        public MemoRandomMappingProfile()
        {
            #region Старый блок - для работы с СУБД
            CreateMap<DbReason, Reason>().ReverseMap();
            CreateMap<DbCategory, Category>().ReverseMap();
            CreateMap<DbComparedHuman, ComparedHuman>().ReverseMap();
            CreateMap<DbHuman, Human>().ReverseMap();
            #endregion

            #region Новый блок - для работы с XML-файлами
            CreateMap<DtoReason, Reason>().ReverseMap();
            CreateMap<DtoCategory, Category>().ReverseMap();
            CreateMap<DtoComparedHuman, ComparedHuman>().ReverseMap();
            CreateMap<DtoHuman, Human>().ReverseMap();
            #endregion
        }
    }
}
