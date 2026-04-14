using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;

namespace Service.Services
{
    public class SchoolRegisterService : IRegister<SchoolRegisterDto, School>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;

        public SchoolRegisterService(IRepository<School> repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<School> Register(SchoolRegisterDto item)
        {
            var entity = mapper.Map<School>(item);

            entity.Name = item.Name.Trim().ToLower();
            entity.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);
            entity.Role = "School";

            var savedSchool = await repository.AddItem(entity);

            return savedSchool;
        }
    }
}