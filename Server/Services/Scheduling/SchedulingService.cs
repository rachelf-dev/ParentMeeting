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
                .Where(pa => pa.Parent.SchoolId == schoolId)
                .ToListAsync();

            // 2. הכנת תשתיות
            var timeSlots = GenerateTimeSlots(school);
            var slotDuration = school.SlotDurationMinutes;
            var teacherBusySlots = teachers.ToDictionary(t => t.Id, t => new HashSet<TimeSpan>());
            var parentAvailabilityDict = parentConstraints.GroupBy(pc => pc.ParentId).ToDictionary(g => g.Key, g => g.ToList());

            var allMeetingsDto = new List<ParentMeetingDto>();

            // מיון הורים לפי מורכבות (יותר ילדים קודם)
            var parentGroups = students
                .Where(s => s.TeacherId != null)
                .GroupBy(s => s.ParentId)
                .OrderByDescending(g => g.Count())
                .ToList();

            // 3. אלגוריתם שיבוץ עם מנגנון הבטחת שיבוץ (Fallback)
            foreach (var group in parentGroups)
            {
                var parentId = group.Key;
                var parentStudents = group.ToList();
                List<TimeSpan> finalSlots = null;

                // ניסיון 1: שיבוץ מושלם (מכבד True ומכבד False)
                finalSlots = FindBestSlotsForFamily(parentId, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, true, true);

                // ניסיון 2: אם נכשל - מוותרים על ה-TRUE (השעות המועדפות) אך עדיין נמנעים מה-FALSE (השעות החסומות)
                if (finalSlots == null)
                    finalSlots = FindBestSlotsForFamily(parentId, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, false, true);

                // ניסיון 3: מוצא אחרון - מוותרים על כל אילוצי ההורה כדי להבטיח שיבוץ לתלמיד
                if (finalSlots == null)
                    finalSlots = FindBestSlotsForFamily(parentId, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, false, false);

                // ביצוע השיבוץ בפועל
                if (finalSlots != null)
                {
                    for (int i = 0; i < parentStudents.Count; i++)
                    {
                        var slot = finalSlots[i];
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

        private List<TimeSpan> FindBestSlotsForFamily(int parentId, List<Student> students, List<TimeSpan> allSlots, Dictionary<int, HashSet<TimeSpan>> teacherBusy, Dictionary<int, List<ParentAvailability>> availDict, int duration, bool usePositive, bool useNegative)
        {
            List<TimeSpan> bestMatch = null;
            double minGap = double.MaxValue;

            // סורקים כל נקודת התחלה אפשרית כדי למצוא את הרצף הכי צפוף (מינימום חורים)
            foreach (var startTime in allSlots)
            {
                var trialSlots = new List<TimeSpan>();
                bool canFitAll = true;

                foreach (var student in students)
                {
                    var slot = allSlots
                        .Where(s => s >= startTime)
                        .Where(s => !teacherBusy[student.TeacherId.Value].Contains(s))
                        .Where(s => !trialSlots.Contains(s))
                        .Where(s => IsSlotAllowed(parentId, s, availDict, duration, usePositive, useNegative))
                        .OrderBy(s => s)
                        .FirstOrDefault();

                    if (slot == default) { canFitAll = false; break; }
                    trialSlots.Add(slot);
                }

                if (canFitAll)
                {
                    var currentGap = (trialSlots.Max() - trialSlots.Min()).TotalMinutes;
                    if (currentGap < minGap)
                    {
                        minGap = currentGap;
                        bestMatch = new List<TimeSpan>(trialSlots);
                    }
                    // אם מצאנו רצף מושלם ללא חורים, נצא מהלופ
                    if (currentGap == (students.Count - 1) * duration) break;
                }
            }
            return bestMatch;
        }

        private bool IsSlotAllowed(int parentId, TimeSpan slot, Dictionary<int, List<ParentAvailability>> dict, int duration, bool usePositive, bool useNegative)
        {
            if (!dict.ContainsKey(parentId)) return true;

            var constraints = dict[parentId];
            var endOfSlot = slot.Add(TimeSpan.FromMinutes(duration));

            // בדיקת אילוצים שליליים (False) - "אני לא יכול בשעות האלו"
            if (useNegative)
            {
                bool isBlocked = constraints.Any(c => !c.IsAvailable &&
                    ((slot >= c.StartTime && slot < c.EndTime) || (endOfSlot > c.StartTime && endOfSlot <= c.EndTime)));
                if (isBlocked) return false;
            }

            // בדיקת אילוצים חיוביים (True) - "אני יכול רק בשעות האלו"
            if (usePositive)
            {
                var positiveConstraints = constraints.Where(c => c.IsAvailable).ToList();
                if (positiveConstraints.Any())
                {
                    bool isInAllowedRange = positiveConstraints.Any(c => slot >= c.StartTime && endOfSlot <= c.EndTime);
                    if (!isInAllowedRange) return false;
                }
            }

            return true;
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