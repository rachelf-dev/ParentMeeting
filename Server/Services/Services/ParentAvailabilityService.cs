using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;

namespace Service.Services
{
    internal class ParentAvailabilityService : IService<ParentAvailabilityDto>
    {
        private readonly IRepository<ParentAvailability> repository;
        private readonly IMapper mapper;
        public ParentAvailabilityService(IRepository<ParentAvailability> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<ParentAvailabilityDto> AddItem(ParentAvailabilityDto item)
        {
            var entity = mapper.Map<ParentAvailability>(item);

            var savedEntity = await repository.AddItem(entity);

            return mapper.Map<ParentAvailabilityDto>(savedEntity);
        }


        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<ParentAvailabilityDto>> GetAll()
        {
            var entities = await repository.GetAll();

            return mapper.Map<List<ParentAvailabilityDto>>(entities);
        }


        public async Task<ParentAvailabilityDto> GetById(int id)
        {
            var parentsAvailability = await repository.GetById(id);

            return mapper.Map<ParentAvailabilityDto>(parentsAvailability);
        }

        public async Task<ParentAvailabilityDto> UpdateItem(int id, ParentAvailabilityDto item)
        {
            var entity = mapper.Map<ParentAvailability>(item);

            var result = await repository.UpdateItem(id, entity);

            return mapper.Map<ParentAvailabilityDto>(result);
        }
    }
}
