using Repository.Entities;
using Service.Dto;
using Service.Interfaces;
using AutoMapper;
using Repository.Interfaces; 
namespace Service.Services
{
    public class ParentService : IService<ParentDto>
    {
        private readonly IRepository<Parent> repository;
        private readonly IMapper mapper;
        public ParentService(IRepository<Parent> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }
        public async Task<ParentDto> AddItem(ParentDto item)
        {
            var parentEntity = mapper.Map<Parent>(item);

            var savedParent = await repository.AddItem(parentEntity);

            return mapper.Map<ParentDto>(savedParent);
        }
        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<ParentDto>> GetAll()
        {
            var parents = await repository.GetAll();

            return mapper.Map<List<ParentDto>>(parents);
        }

        public async Task<ParentDto> GetById(int id)
        {
            var parent = await repository.GetById(id);

            return mapper.Map<ParentDto>(parent);
        }

        public async Task<ParentDto> UpdateItem(int id, ParentDto item)
        {
            var entity = mapper.Map<Parent>(item);

            var result = await repository.UpdateItem(id, entity);

            return mapper.Map<ParentDto>(result);
        }
    }
}