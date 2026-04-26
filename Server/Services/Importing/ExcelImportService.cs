using DataContext;
using OfficeOpenXml;
using Repository;
using Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace Service.Importing
{
    public class ExcelImportService
    {
        private readonly SchoolParentMeetingSystemContext _context;

        public ExcelImportService(SchoolParentMeetingSystemContext context)
        {
            _context = context;
        }

        public async Task ImportFromExcel(Stream fileStream, int schoolId)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new ExcelPackage(fileStream);

            if (package.Workbook.Worksheets.Count == 0)
                throw new Exception("Excel file is empty");

            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension.Rows;

            // --- 1. ניקוי נתונים קיימים (מחיקת הכל כדי להתחיל דף חדש) ---
            // מחיקת פגישות קיימות
            var meetingsToRemove = _context.ParentMeetings.Where(m => m.SchoolId == schoolId);
            _context.ParentMeetings.RemoveRange(meetingsToRemove);

            //// מחיקת אילוצי הורים (Availability)
            //var constraintsToRemove = _context.ParentAvailability.Where(pa => pa.Parent.SchoolId == schoolId);
            //_context.ParentAvailability.RemoveRange(constraintsToRemove);

            // מוחקים קודם תלמידים (כי הם תלויים בהורים ומורים)
            var studentsToRemove = _context.Students.Where(s => s.SchoolId == schoolId);
            _context.Students.RemoveRange(studentsToRemove);

            // מוחקים מורים
            var teachersToRemove = _context.Teachers.Where(t => t.SchoolId == schoolId);
            _context.Teachers.RemoveRange(teachersToRemove);

            // מוחקים הורים (כדי למנוע שגיאת Email/Identity כפול)
            var parentsToRemove = _context.Parents.Where(p => p.SchoolId == schoolId);
            _context.Parents.RemoveRange(parentsToRemove);

            // שמירה זמנית של המחיקה
            await _context.SaveChangesAsync();

            // --- 2. הכנה לייבוא החדש ---
            var parentsDict = new Dictionary<string, Parent>();
            var teachersDict = new Dictionary<string, Teacher>();

            for (int row = 2; row <= rowCount; row++)
            {
                var firstName = worksheet.Cells[row, 1].Text.Trim();
                var lastName = worksheet.Cells[row, 2].Text.Trim();
                var studentIdentity = worksheet.Cells[row, 3].Text.Trim();
                var className = worksheet.Cells[row, 4].Text.Trim();
                var teacherName = worksheet.Cells[row, 5].Text.Trim();
                var parentIdentity = worksheet.Cells[row, 6].Text.Trim();
                var parentName = worksheet.Cells[row, 7].Text.Trim();
                var parentEmail = worksheet.Cells[row, 8].Text.Trim();

                // דילוג על שורות ריקות בטעות
                if (string.IsNullOrEmpty(studentIdentity) || string.IsNullOrEmpty(parentEmail))
                    continue;

                // --- טיפול בהורה (מניעת כפילות באותו קובץ) ---
                if (!parentsDict.TryGetValue(parentIdentity, out var parent))
                {
                    parent = new Parent
                    {
                        ParentIdentity = parentIdentity,
                        ParentName = parentName,
                        ParentEmail = parentEmail,
                        SchoolId = schoolId
                    };

                    _context.Parents.Add(parent);
                    parentsDict[parentIdentity] = parent;
                }

                // --- טיפול במורה (לפי כיתה) ---
                if (!teachersDict.TryGetValue(className, out var teacher))
                {
                    teacher = new Teacher
                    {
                        FullName = teacherName,
                        ClassName = className,
                        SchoolId = schoolId
                    };

                    _context.Teachers.Add(teacher);
                    teachersDict[className] = teacher;
                }

                // --- טיפול בתלמיד ---
                var student = new Student
                {
                    StudentIdentity = studentIdentity,
                    FirstName = firstName,
                    LastName = lastName,
                    ClassName = className,
                    Parent = parent, 
                    Teacher = teacher,  
                    SchoolId = schoolId
                };

                _context.Students.Add(student);
            }

            await _context.SaveChangesAsync();
        }
    }
}