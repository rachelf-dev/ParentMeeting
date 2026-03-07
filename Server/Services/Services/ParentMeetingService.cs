using AutoMapper;
using Repository.Entities;
using Repository.Interfaces;
using Service.Dto;
using Service.Interfaces;


namespace Service.Services
{
    public class ParentMeetingService : IService<ParentMeetingDto>
    {
        private readonly IRepository<ParentMeeting> repository;
        private readonly IMapper mapper;
        public ParentMeetingService(IRepository<ParentMeeting> repository, IMapper map)
        {
            this.repository = repository;
            this.mapper = map;
        }

        public async Task<ParentMeetingDto> AddItem(ParentMeetingDto item)
        {
            var entity = mapper.Map<ParentMeeting>(item);

            var savedEntity = await repository.AddItem(entity);

            return mapper.Map<ParentMeetingDto>(savedEntity);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<ParentMeetingDto>> GetAll()
        {
            var parentsMeeting = await repository.GetAll();

            return mapper.Map<List<ParentMeetingDto>>(parentsMeeting);
        }

        public async Task<ParentMeetingDto> GetById(int id)
        {
            var parentMeeting = await repository.GetById(id);

            return mapper.Map<ParentMeetingDto>(parentMeeting);
        }

        public async Task<ParentMeetingDto> UpdateItem(int id, ParentMeetingDto item)
        {
            var entity = mapper.Map<ParentMeeting>(item);

            var result = await repository.UpdateItem(id, entity);

            return mapper.Map<ParentMeetingDto>(result);
        }
    }
}
