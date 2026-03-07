using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class SchoolRegisterService : IRegister<SchoolRegisterDto>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;

        public SchoolRegisterService(IRepository<School> repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<SchoolRegisterDto> Register(SchoolRegisterDto item)
        {
            var entity = mapper.Map<School>(item);

            entity.Password = BCrypt.Net.BCrypt.HashPassword(item.Password);

            var savedSchool = await repository.AddItem(entity);

            return mapper.Map<SchoolRegisterDto>(savedSchool);
        }

    }

}
