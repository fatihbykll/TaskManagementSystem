using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;
namespace TaskManagement.Application.Mappings;
public class CommentAttachmentMappingProfile : Profile
{
    public CommentAttachmentMappingProfile()
    {
        // Entity'de alan adı "Comment", DTO'da "Content"; explicit mapping zorunludur.
        CreateMap<TaskComment, CommentDto>()
            .ForMember(d => d.Content, o => o.MapFrom(s => s.Comment));
        CreateMap<TaskAttachment, TaskAttachmentDto>();
    }
}
