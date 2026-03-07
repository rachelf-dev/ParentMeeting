using Microsoft.EntityFrameworkCore;
using Repository.Entities;



namespace Repository.Interfaces
{
    public interface IContext
    {
        public DbSet<Student> Students { set; get; }
        public DbSet<Teacher> Teachers { set; get; }
        public DbSet<ParentMeeting> ParentMeetings { set; get; }
        public DbSet<ParentAvailability> ParentAvailability { set; get; }
        public DbSet<Parent> Parents { set; get; }
        public DbSet<School> Schools { set; get; }
        public Task Save();
    }
}
