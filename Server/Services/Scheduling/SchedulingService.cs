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
            // 1. שליפת נתונים בסיסיים
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

            // 2. הכנת תשתיות לשיבוץ
            var timeSlots = GenerateTimeSlots(school);
            var slotDuration = school.SlotDurationMinutes;

            var teacherBusySlots = teachers
                .ToDictionary(t => t.Id, t => new HashSet<TimeSpan>());

            var parentBlockedSlots = parentConstraints
                .GroupBy(pc => pc.ParentId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allMeetingsDto = new List<ParentMeetingDto>();

            // מיון הורים לפי כמות ילדים (המורכבים ביותר קודם)
            var parentGroups = students
                .Where(s => s.TeacherId != null)
                .GroupBy(s => s.ParentId)
                .OrderByDescending(g => g.Count())
                .ToList();

            // 3. אלגוריתם שיבוץ מבוסס "חלון זמן משפחתי"
            foreach (var group in parentGroups)
            {
                var parentId = group.Key;
                var parentStudents = group.ToList();

                List<TimeSpan> bestFamilySlots = null;
                double minFamilyPenalty = double.MaxValue;

                // ננסה כל סלוט אפשרי כנקודת התחלה פוטנציאלית עבור המשפחה
                foreach (var startTime in timeSlots)
                {
                    var currentTrialSlots = new List<TimeSpan>();
                    bool canScheduleAll = true;

                    foreach (var student in parentStudents)
                    {
                        // מחפשים עבור הילד הנוכחי את הסלוט הפנוי הכי קרוב ל-startTime
                        // (חייב להיות פנוי גם למורה וגם להורה)
                        var availableSlot = timeSlots
                            .Where(s => s >= startTime)
                            .Where(s => !teacherBusySlots[student.TeacherId.Value].Contains(s))
                            .Where(s => !currentTrialSlots.Contains(s)) // שההורה לא בשתי פגישות במקביל
                            .Where(s => !IsParentBlocked(parentId, s, parentBlockedSlots, slotDuration))
                            .OrderBy(s => s)
                            .FirstOrDefault();

                        if (availableSlot == default && startTime != timeSlots.Last())
                        {
                            canScheduleAll = false;
                            break;
                        }

                        currentTrialSlots.Add(availableSlot);
                    }

                    if (canScheduleAll && currentTrialSlots.All(s => s != default))
                    {
                        // חישוב "קנס" המשפחה: ההפרש בדקות בין הפגישה הראשונה לאחרונה
                        var totalTimeGap = (currentTrialSlots.Max() - currentTrialSlots.Min()).TotalMinutes;

                        if (totalTimeGap < minFamilyPenalty)
                        {
                            minFamilyPenalty = totalTimeGap;
                            bestFamilySlots = new List<TimeSpan>(currentTrialSlots);
                        }

                        // אם מצאנו רצף מושלם (ללא חורים), נשתמש בו ונעבור למשפחה הבאה
                        if (totalTimeGap == (parentStudents.Count - 1) * slotDuration)
                            break;
                    }
                }

                // ביצוע השיבוץ הסופי עבור המשפחה
                if (bestFamilySlots != null)
                {
                    for (int i = 0; i < parentStudents.Count; i++)
                    {
                        var slot = bestFamilySlots[i];
                        var student = parentStudents[i];

                        allMeetingsDto.Add(CreateDto(school, student, slot));
                        teacherBusySlots[student.TeacherId.Value].Add(slot);
                    }
                }
            }

            // 4. שמירה במסד נתונים
            await SaveToDatabaseAsync(schoolId, allMeetingsDto);

            return allMeetingsDto;
        }

        private bool IsParentBlocked(int parentId, TimeSpan slot, Dictionary<int, List<ParentAvailability>> blockedDict, int duration)
        {
            if (!blockedDict.ContainsKey(parentId)) return false;

            var endOfSlot = slot.Add(TimeSpan.FromMinutes(duration));
            return blockedDict[parentId].Any(c =>
                (slot >= c.StartTime && slot < c.EndTime) ||
                (endOfSlot > c.StartTime && endOfSlot <= c.EndTime));
        }

        private List<TimeSpan> GenerateTimeSlots(School school)
        {
            var slots = new List<TimeSpan>();
            if (school.SlotDurationMinutes <= 0) return slots;

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