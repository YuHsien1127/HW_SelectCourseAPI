using MathNet.Numerics.Distributions;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;

namespace SelectCourseAPI.Services
{
    public class ExcelService : IExcelService
    {
        private readonly ILogger<ExcelService> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        public ExcelService(ILogger<ExcelService> logger, IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository)
        {
            _logger = logger;
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
        }

        public byte[] ExcelEnroll_CourseByStudentId(int studentId)
        {
            _logger.LogInformation("開始匯出StudentId：{studentId}選的課程", studentId);
            if(studentId == 0)
            {
                _logger.LogWarning("Student為Null");
                return Array.Empty<byte>();
            }
            var student = _studentRepository.GetStudentById(studentId);
            var enroll = _enrollmentRepository.GetAllEnrollments().Where(s => s.StudentId == studentId).ToList();
            // 產生 Excel
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Student " + studentId + "_Course");

            // 標題列
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("CourseId");
            headerRow.CreateCell(1).SetCellValue("Code");
            headerRow.CreateCell(2).SetCellValue("Title");
            headerRow.CreateCell(3).SetCellValue("Grade");
            headerRow.CreateCell(4).SetCellValue("LetterGrade");
            headerRow.CreateCell(5).SetCellValue("GradePoint");
            headerRow.CreateCell(6).SetCellValue("Crediits");
            headerRow.CreateCell(7).SetCellValue("IsActive");
            headerRow.CreateCell(8).SetCellValue("IsDel");
            headerRow.CreateCell(9).SetCellValue("CreatedAt");
            headerRow.CreateCell(10).SetCellValue("UpdatedAt");
            headerRow.CreateCell(11).SetCellValue("Status");

            if (student == null)
            {
                _logger.LogWarning("無此StudentId：{studentId}學生", studentId);
                return Array.Empty<byte>();
            }
            else if (student != null && !enroll.Any())
            {
                _logger.LogWarning("此學生還未選課（StudentId：{studentId}）", studentId);
            }
            else
            {
                // 填資料
                var rowIndex = 1;
                foreach (var query in enroll)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(query.CourseId);
                    row.CreateCell(1).SetCellValue(query.Course.Code);
                    row.CreateCell(2).SetCellValue(query.Course.Title);
                    row.CreateCell(3).SetCellValue(query.Grade?.ToString() ?? "NULL");                    
                    row.CreateCell(4).SetCellValue(query.LetterGrade ?? "NULL");
                    row.CreateCell(5).SetCellValue(query.GradePoint?.ToString() ?? "NULL");
                    row.CreateCell(6).SetCellValue(query.Course.Credits);
                    row.CreateCell(7).SetCellValue(query.Course.IsActive ? "開課" : "停課");
                    row.CreateCell(8).SetCellValue(query.Course.IsDel ? "刪除" : "存在");
                    row.CreateCell(9).SetCellValue(query.CreatedAt.ToString("yyyy/MM/dd(HH:mm:ss)"));
                    row.CreateCell(10).SetCellValue(query.UpdatedAt.ToString("yyyy/MM/dd(HH:mm:ss)"));
                    row.CreateCell(11).SetCellValue(query.Status);
                }
            }
            // 自動調整欄寬
            for (int col = 0; col <= 11; col++)
                sheet.AutoSizeColumn(col);
            // 轉成 byte[]
            using var ms = new MemoryStream();
            workbook.Write(ms, leaveOpen: true); // 避免 Stream 被關閉
            _logger.LogInformation("匯出完成，StudentId：{studentId}，筆數：{Count}，檔案大小：{Size} bytes"
                               , studentId, enroll.Count, ms.Length);
            return ms.ToArray();
        }
        public byte[] ExcelEnroll_StudentByCourseId(int courseId)
        {
            _logger.LogInformation("開始匯出CourseId：{courseId}課程的學生", courseId);
            if (courseId == 0)
            {
                _logger.LogWarning("Course為Null");
                return Array.Empty<byte>();
            }
            var course = _courseRepository.GetCourseById(courseId);
            var enroll = _enrollmentRepository.GetAllEnrollments().Where(s => s.CourseId == courseId).ToList();
            // 產生 Excel
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Course " + courseId + "_Student");

            // 標題列
            IRow headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("StudentId");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Email");
            headerRow.CreateCell(3).SetCellValue("CreatedAt");
            headerRow.CreateCell(4).SetCellValue("UpdatedAt");
            headerRow.CreateCell(5).SetCellValue("Status");

            if (course == null)
            {
                _logger.LogWarning("無此CourseId：{courseId}學生", courseId);
                return Array.Empty<byte>();
            }
            else if (course != null && !enroll.Any())
            {
                _logger.LogWarning("此學生還未選課（CourseId：{courseId}）", courseId);
            }
            else
            {
                // 填資料
                var rowIndex = 1;
                foreach (var query in enroll)
                {
                    var row = sheet.CreateRow(rowIndex++);
                    row.CreateCell(0).SetCellValue(query.StudentId);
                    row.CreateCell(1).SetCellValue(query.Student.LastName + query.Student.FirstName);
                    row.CreateCell(2).SetCellValue(query.Student.Email);
                    row.CreateCell(3).SetCellValue(query.CreatedAt.ToString("yyyy/MM/dd(HH:mm:ss)"));
                    row.CreateCell(4).SetCellValue(query.UpdatedAt.ToString("yyyy/MM/dd(HH:mm:ss)"));
                    row.CreateCell(5).SetCellValue(query.Status);
                }
            }
            // 自動調整欄寬
            for (int col = 0; col <= 5; col++)
                sheet.AutoSizeColumn(col);
            // 轉成 byte[]
            using var ms = new MemoryStream();
            workbook.Write(ms, leaveOpen: true); // 避免 Stream 被關閉
            _logger.LogInformation("匯出完成，CourseId：{courseId}，筆數：{Count}，檔案大小：{Size} bytes"
                               , courseId, enroll.Count, ms.Length);
            return ms.ToArray();
        }

    }
}
