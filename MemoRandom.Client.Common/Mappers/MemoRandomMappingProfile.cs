using AutoMapper;
using MemoRandom.Data.DbModels;

namespace MemoRandom.Client.Common.Mappers
{
    public class MemoRandomMappingProfile : Profile
    {
        public MemoRandomMappingProfile()
        {
            //CreateMap<DbReason, Reason>().ReverseMap();
                //.ForMember(x => x.ReasonId, x => x.MapFrom(cfg => cfg.DbReasonId))
                //.ForMember(x => x.ReasonName, x => x.MapFrom(cfg => cfg.DbReasonName))
                //.ForMember(x => x.ReasonComment, x => x.MapFrom(cfg => cfg.DbReasonComment))
                //.ForMember(x => x.ReasonDescription, x => x.MapFrom(cfg => cfg.DbReasonDescription))
                //.ForMember(x => x.ReasonParentId, x => x.MapFrom(cfg => cfg.DbReasonParentId))
                //.ForMember(x => x.ReasonParent, x => x.Ignore())
                //.ForMember(x => x.ReasonChildren, x => x.Ignore())
                /*.ForMember(x => x.IsSelected, x => x.Ignore())*//*.ReverseMap();*/

            //CreateMap<DbCategory, Category>();
        }
    }
}
