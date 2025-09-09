using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Services;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "admit, user")]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        /// <summary>
        /// 取得全部 Enrollment
        /// </summary>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">幾筆</param>
        /// <returns></returns>
        [HttpGet]
        public EnrollmentResponse GetAllEnrollment(int page = 1, int pageSize = 10)
        {
            return _enrollmentService.GetAllEnrollments(page, pageSize);
        }

        /// <summary>
        /// 根據 studentId 和 courseId 取得 Enrollment
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpGet]
        public EnrollmentResponse GetEnrollment(int courseId = 0)
        {
            return _enrollmentService.GetEnrollmentById(courseId);
        }

        /// <summary>
        /// 選課
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpPost]
        public EnrollmentResponse Enroll(int courseId = 0)
        {
            return _enrollmentService.Enroll(courseId);
        }

        /// <summary>
        /// 更新成績
        /// </summary>
        /// <param name="enrollmentRequest">更新資料</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "admit")]
        public EnrollmentResponse UpdateGrade([FromBody] EnrollmentRequest enrollmentRequest)
        {
            return _enrollmentService.UpdateGrade(enrollmentRequest);
        }
        /// <summary>
        /// 退選
        /// </summary>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpDelete]
        public EnrollmentResponse Withdraw(int courseId = 0)
        {
            return _enrollmentService.Withdraw(courseId);
        }
    }
}
