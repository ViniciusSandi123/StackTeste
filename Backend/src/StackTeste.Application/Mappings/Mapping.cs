using AutoMapper;
using StackTeste.Application.DTOs.Lead;
using StackTeste.Application.DTOs.Task;
using StackTeste.Domain.Helpers.Enums;
using StackTeste.Domain.Models;
using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;
using TaskCountByLeadId = System.Collections.Generic.IReadOnlyDictionary<int, int>;

namespace StackTeste.Application.Mappings
{
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Lead, LeadDto>()
                .ForCtorParam(nameof(LeadDto.TasksCount), opt =>
                    opt.MapFrom((src, ctx) => ResolveTaskCount(src, ctx)));

            CreateMap<Lead, LeadDetailDto>()
                .ForCtorParam(nameof(LeadDetailDto.Tasks), opt =>
                    opt.MapFrom(src => src.Tasks ?? new List<TaskItem>()));

            CreateMap<LeadCreateDto, Lead>()
                .ForMember(dest => dest.Status, opt =>
                    opt.MapFrom(src => src.Status ?? LeadStatus.New));

            CreateMap<LeadUpdateDto, Lead>();

            CreateMap<TaskItem, TaskDto>();

            CreateMap<TaskCreateDto, TaskItem>()
                .ForMember(dest => dest.Status, opt =>
                    opt.MapFrom(src => src.Status ?? TaskStatus.Todo));

            CreateMap<TaskUpdateDto, TaskItem>();
        }

        private static int ResolveTaskCount(Lead src, ResolutionContext ctx)
        {
            if (ctx.TryGetItems(out var items) &&
                items.TryGetValue("taskCounts", out var counts) &&
                counts is TaskCountByLeadId dict)
                return dict.GetValueOrDefault(src.Id, 0);

            return src.Tasks?.Count ?? 0;
        }
    }
}