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

            if (school == null) throw new Exception("בית הספר לא נמצא.");

            var students = await _context.Students
                .AsNoTracking()
                .Where(s => s.SchoolId == schoolId)
                .ToListAsync();

            var teachers = await _context.Teachers
                .AsNoTracking()
                .Where(t => t.SchoolId == schoolId)
                .ToListAsync();

            // שליפת כל ההורים הקיימים בבית הספר לצורך בדיקת קיום
            var existingParentIdentities = await _context.Parents
                .Where(p => p.SchoolId == schoolId)
                .Select(p => p.ParentIdentity)
                .AsNoTracking()
                .ToListAsync();

            var parentConstraints = await _context.ParentAvailability
                .AsNoTracking()
                .Where(pa => pa.SchoolId == schoolId)
                .ToListAsync();

            // 2. הכנת תשתיות - קיבוץ לפי תעודת זהות (string)
            var timeSlots = GenerateTimeSlots(school);
            var slotDuration = school.SlotDurationMinutes;
            var teacherBusySlots = teachers.ToDictionary(t => t.Id, t => new HashSet<TimeSpan>());

            var parentAvailabilityDict = parentConstraints
                .GroupBy(pc => pc.ParentIdentity)
                .ToDictionary(g => g.Key, g => g.ToList());

            var allMeetingsDto = new List<ParentMeetingDto>();

            // מיון קבוצות לפי הורה
            var parentGroups = students
                .Where(s => s.TeacherId != null)
                .GroupBy(s => s.ParentId.ToString()) // קיבוץ לפי ה-Identity שנמצא ב-Student
                .OrderByDescending(g => g.Count())
                .ToList();

            // 3. אלגוריתם שיבוץ
            foreach (var group in parentGroups)
            {
                var parentIdentity = group.Key;
                var parentStudents = group.ToList();
                List<TimeSpan> finalSlots = null;

                // בדיקה: האם ההורה בכלל קיים במערכת?
                bool parentExists = existingParentIdentities.Contains(parentIdentity);

                if (parentExists)
                {
                    // ניסיון 1: שיבוץ עם כל האילוצים
                    finalSlots = FindBestSlotsForFamily(parentIdentity, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, true, true);

                    // ניסיון 2: ויתור על שעות מועדפות (Positive)
                    if (finalSlots == null)
                        finalSlots = FindBestSlotsForFamily(parentIdentity, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, false, true);
                }

                // ניסיון 3: שיבוץ ללא אילוצים (מופעל אם ההורה לא קיים או אם הניסיונות הקודמים נכשלו)
                if (finalSlots == null)
                    finalSlots = FindBestSlotsForFamily(parentIdentity, parentStudents, timeSlots, teacherBusySlots, parentAvailabilityDict, slotDuration, false, false);

                // ביצוע השיבוץ בפועל
                if (finalSlots != null)
                {
                    for (int i = 0; i < parentStudents.Count; i++)
                    {
                        var student = parentStudents[i];
                        var slot = finalSlots[i];

                        allMeetingsDto.Add(CreateDto(school, student, slot));
                        teacherBusySlots[student.TeacherId.Value].Add(slot);
                    }
                }
            }

            await SaveToDatabaseAsync(schoolId, allMeetingsDto);
            return allMeetingsDto;
        }

        private List<TimeSpan> FindBestSlotsForFamily(string parentIdentity, List<Student> students, List<TimeSpan> allSlots, Dictionary<int, HashSet<TimeSpan>> teacherBusy, Dictionary<string, List<ParentAvailability>> availDict, int duration, bool usePositive, bool useNegative)
        {
            List<TimeSpan> bestMatch = null;
            double minGap = double.MaxValue;

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
                        .Where(s => IsSlotAllowed(parentIdentity, s, availDict, duration, usePositive, useNegative))
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
                    if (currentGap == (students.Count - 1) * duration) break;
                }
            }
            return bestMatch;
        }

        private bool IsSlotAllowed(string parentIdentity, TimeSpan slot, Dictionary<string, List<ParentAvailability>> dict, int duration, bool usePositive, bool useNegative)
        {
            // אם אין אילוצים מתועדים לת"ז הזו, הכל מותר
            if (string.IsNullOrEmpty(parentIdentity) || !dict.ContainsKey(parentIdentity)) return true;

            var constraints = dict[parentIdentity];
            var endOfSlot = slot.Add(TimeSpan.FromMinutes(duration));

            if (useNegative)
            {
                bool isBlocked = constraints.Any(c => !c.IsAvailable &&
                    ((slot >= c.StartTime && slot < c.EndTime) || (endOfSlot > c.StartTime && endOfSlot <= c.EndTime)));
                if (isBlocked) return false;
            }

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
            var duration = TimeSpan.FromMinutes(school.SlotDurationMinutes);
            for (var t = school.MeetingStartTime; t + duration <= school.MeetingEndTime; t += duration)
                slots.Add(t);
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
                EndTime = slot.Add(TimeSpan.FromMinutes(school.SlotDurationMinutes))
            };
        }

        private async Task SaveToDatabaseAsync(int schoolId, List<ParentMeetingDto> dtos)
        {
            var existing = await _context.ParentMeetings.Where(m => m.SchoolId == schoolId).ToListAsync();
            _context.ParentMeetings.RemoveRange(existing);
            var entities = dtos.Select(d => new ParentMeeting
            {
                SchoolId = d.SchoolId,
                StudentId = d.StudentId,
                ParentId = d.ParentId,
                ClassName = d.ClassName,
                MeetingDate = d.MeetingDate,
                StartTime = d.StartTime,
                EndTime = d.EndTime
            }).ToList();
            await _context.ParentMeetings.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
    }
}