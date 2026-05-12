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
            // מיפוי פגישת הורים - כאן המורה נמצא!
            CreateMap<ParentMeeting, ParentMeetingDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                    src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : "לא נמצא"))
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src =>
                    src.Parent != null ? src.Parent.ParentName : "לא נמצא"))
                // הוסף את השורה הזו כאן:
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src =>
                    src.Teacher != null ? src.Teacher.FullName : "לא נמצא"))
                .ReverseMap();

            // מיפוי זמינות הורים - כאן אין מורה, לכן הקוד הקודם נכשל
            CreateMap<ParentAvailability, ParentAvailabilityDto>()
                .ReverseMap(); CreateMap<Student, StudentDto>().ReverseMap();
            CreateMap<Teacher, TeacherDto>().ReverseMap();
        }
    }
}
