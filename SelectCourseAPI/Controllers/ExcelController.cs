using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Services;

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _excelService;
        public ExcelController(IExcelService excelService)
        {
            _excelService = excelService;
        }
        /// <summary>
        /// 根據 studentId 匯出他的所選課程
        /// </summary>
        /// <param name="studentId">學生 Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExcelEnroll_CourseByStudentId(int studentId = 0)
        {
            var fileBytes = _excelService.ExcelEnroll_CourseByStudentId(studentId);
            string fileName = studentId > 0 ? $"StudentId{studentId}_課程資料_{DateTime.Now:yyyyMMddHHmmss}.xlsx" : $"Empty_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        /// <summary>
        /// 根據 courseId 匯出選此課程的學生
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ExcelEnroll_StudentByCourseId(int courseId = 0)
        {
            var fileBytes = _excelService.ExcelEnroll_StudentByCourseId(courseId);
            string fileName = courseId > 0 ? $"CourseId{courseId}_學生名單_{DateTime.Now:yyyyMMddHHmmss}.xlsx" : $"Empty_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
