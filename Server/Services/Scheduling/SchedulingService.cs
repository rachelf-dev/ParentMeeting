using DataContext;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Entities;
using Service.Dto;


namespace Service.Scheduling
{
    public interface ISchedulingService
    {
        Task<List<ParentMeetingDto>> ProcessSchedulingAsync(int schoolId);
    }

    public class SchedulingService : ISchedulingService
    {
        private readonly SchoolParentMeetingSystemContext _context;

        public SchedulingService(SchoolParentMeetingSystemContext context)
        {
            _context = context;
        }

        public async Task<List<ParentMeetingDto>> ProcessSchedulingAsync(int schoolId)
        {
            // 1. שליפת נתונים
            var school = await _context.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == schoolId);

            if (school == null)
                throw new Exception("בית הספר לא נמצא.");

            var students = await _context.Students
                .AsNoTracking()
                .Where(s => s.SchoolId == schoolId)
                .ToListAsync();

            var teachers = await _context.Teachers
                .AsNoTracking()
                .Where(t => t.SchoolId == schoolId)
                .ToListAsync();

            var parentConstraints = await _context.ParentAvailability
                .Include(pa => pa.Parent)
                .AsNoTracking()
                .Where(pa => pa.Parent.SchoolId == schoolId && pa.IsAvailable == false)
                .ToListAsync();

            // 2. יצירת סלוטים
            var timeSlots = GenerateTimeSlots(school);

            var teacherBusySlots = teachers
                .ToDictionary(t => t.Id, t => new HashSet<TimeSpan>());

            var parentBusySlots = students
                .Select(s => s.ParentId)
                .Distinct()
                .ToDictionary(p => p, p => new HashSet<TimeSpan>());

            var parentBlockedSlots = parentConstraints
                .GroupBy(pc => pc.ParentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allMeetingsDto = new List<ParentMeetingDto>();

            // מיון הורים לפי מספר ילדים
            var parentGroups = students
                .GroupBy(s => s.ParentId)
                .OrderByDescending(g => g.Count())
                .ToList();

            // 3. אלגוריתם שיבוץ
            foreach (var group in parentGroups)
            {
                var parentId = group.Key;
                var parentStudents = group.ToList();
                var currentParentAssignments = new List<TimeSpan>();

                foreach (var student in parentStudents)
                {
                    if (student.TeacherId == null)
                        continue;

                    TimeSpan? bestSlot = null;
                    double minPenalty = double.MaxValue;

                    foreach (var slot in timeSlots)
                    {
                        if (!teacherBusySlots[student.TeacherId.Value].Contains(slot) &&
                            !parentBusySlots[parentId].Contains(slot) &&
                            !IsParentBlocked(parentId, slot, parentBlockedSlots, school.SlotDurationMinutes))
                        {
                            double penalty = CalculateGapPenalty(slot, currentParentAssignments, school.SlotDurationMinutes);

                            if (penalty < minPenalty)
                            {
                                minPenalty = penalty;
                                bestSlot = slot;
                            }

                            if (penalty == 0)
                                break;
                        }
                    }

                    if (bestSlot.HasValue)
                    {
                        currentParentAssignments.Add(bestSlot.Value);

                        allMeetingsDto.Add(CreateDto(school, student, bestSlot.Value));

                        teacherBusySlots[student.TeacherId.Value].Add(bestSlot.Value);
                        parentBusySlots[parentId].Add(bestSlot.Value);
                    }
                }
            }

            // 4. שמירה במסד נתונים
            await SaveToDatabaseAsync(schoolId, allMeetingsDto);

            return allMeetingsDto;
        }

        private bool IsParentBlocked(int parentId, TimeSpan slot, Dictionary<int, List<ParentAvailability>> blockedDict, int duration)
        {
            if (!blockedDict.ContainsKey(parentId))
                return false;

            var endOfSlot = slot.Add(TimeSpan.FromMinutes(duration));

            return blockedDict[parentId].Any(c =>
                (slot >= c.StartTime && slot < c.EndTime) ||
                (endOfSlot > c.StartTime && endOfSlot <= c.EndTime));
        }

        private double CalculateGapPenalty(TimeSpan currentSlot, List<TimeSpan> existingSlots, int duration)
        {
            if (!existingSlots.Any())
                return 0;

            double minDiffMinutes = existingSlots
                .Select(s => Math.Abs((s - currentSlot).TotalMinutes))
                .Min();

            if (minDiffMinutes <= duration)
                return 0;

            return Math.Pow(minDiffMinutes, 2);
        }

        private List<TimeSpan> GenerateTimeSlots(School school)
        {
            var slots = new List<TimeSpan>();

            var slotDuration = TimeSpan.FromMinutes(school.SlotDurationMinutes);

            for (var t = school.MeetingStartTime; t + slotDuration <= school.MeetingEndTime; t += slotDuration)
            {
                slots.Add(t);
            }

            return slots;
        }

        private ParentMeetingDto CreateDto(School school, Student student, TimeSpan slot)
        {
            return new ParentMeetingDto
            {
                SchoolId = school.Id,
                StudentId = student.Id,
                ParentId = student.ParentId,
                ClassName = student.ClassName,
                MeetingDate = school.MeetingDate,
                StartTime = slot,
                EndTime = slot.Add(TimeSpan.FromMinutes(school.SlotDurationMinutes)),
                IsPast = false
            };
        }

        private async Task SaveToDatabaseAsync(int schoolId, List<ParentMeetingDto> dtos)
        {
            var existing = await _context.ParentMeetings
                .Where(m => m.SchoolId == schoolId)
                .ToListAsync();

            _context.ParentMeetings.RemoveRange(existing);

            var entities = dtos.Select(d => new ParentMeeting
            {
                SchoolId = d.SchoolId,
                StudentId = d.StudentId,
                ParentId = d.ParentId,
                ClassName = d.ClassName,
                MeetingDate = d.MeetingDate,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                IsPast = false
            }).ToList();

            await _context.ParentMeetings.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}