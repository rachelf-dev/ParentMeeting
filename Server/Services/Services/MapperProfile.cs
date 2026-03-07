using AutoMapper;

using Repository;
using Repository.Entities;
using Service.Dto;


namespace SchoolParentMeetingSystem.Service.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {

            CreateMap<School, SchoolRegisterDto>().ReverseMap();
            CreateMap<School, SchoolDto>().ReverseMap();
            CreateMap<School, SchoolLoginDto>().ReverseMap();
            CreateMap<Parent, ParentDto>().ReverseMap();
            CreateMap<ParentMeeting, ParentMeetingDto>().ReverseMap();
            CreateMap<ParentAvailability, ParentAvailabilityDto>().ReverseMap();
            CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Teacher, TeacherDto>().ReverseMap();
        }
    }
}
