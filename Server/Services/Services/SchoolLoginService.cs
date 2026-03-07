using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;


namespace Service.Services
{
    public class SchoolLoginService : ILogin<SchoolLoginDto>
    {
        private readonly IRepository<School> repository;
        private readonly IMapper mapper;
        private readonly IToken<School> _tokenService;

        public SchoolLoginService(IRepository<School> repository, IMapper mapper, IToken<School> token)
        {
            this.repository = repository;
            this.mapper = mapper;
            this._tokenService = token;
        }

        public async Task<SchoolLoginDto> Login(SchoolLoginDto item)
        {
            var allSchools = await repository.GetAll();

            var school = allSchools.FirstOrDefault(s =>
                s.Name == item.Name );

            if (school == null)
                throw new Exception("Invalid name or password");

            bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(item.Password, school.Password);

            if (!isPasswordCorrect)
                throw new Exception("Invalid name or password");

            var schoolDto = mapper.Map<SchoolLoginDto>(school);

            schoolDto.Token = _tokenService.GenerateToken(school);

            return schoolDto;
        }
    }
}


