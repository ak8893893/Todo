using AutoMapper;
using Todo.Dto;
using Todo.Models;

namespace Todo.Profiles
{
    public class UploadFileProfile : Profile
    {
        public UploadFileProfile()
        {
            CreateMap<UploadFile, UploadFileDto>();
        }
    }
}
