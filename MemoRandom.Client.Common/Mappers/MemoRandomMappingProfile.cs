using AutoMapper;
using MemoRandom.Client.Common.Models;
using MemoRandom.Data.DtoModels;

namespace MemoRandom.Client.Common.Mappers
{
    /// <summary>
    /// Профиль работы с маппером
    /// </summary>
    public class MemoRandomMappingProfile : Profile
    {
        public MemoRandomMappingProfile()
        {
            #region Новый блок - для работы с XML-файлами
            CreateMap<DtoReason, Reason>().ReverseMap();
            CreateMap<DtoCategory, Category>().ReverseMap();
            CreateMap<DtoComparedHuman, ComparedHuman>().ReverseMap();
            CreateMap<DtoHuman, Human>().ReverseMap();
            #endregion
        }
    }
}
