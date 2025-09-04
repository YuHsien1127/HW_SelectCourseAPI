using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        /// <returns></returns>
        [HttpGet]
        public EnrollmentResponse GetAllEnrollment()
        {
            return _enrollmentService.GetAllEnrollments();
        }

        /// <summary>
        /// 根據 studentId 和 courseId 取得 Enrollment
        /// </summary>
        /// <param name="studentId">學生 Id</param>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpGet]
        public EnrollmentResponse GetEnrollment(int studentId = 0, int courseId = 0)
        {
            return _enrollmentService.GetEnrollmentById(studentId, courseId);
        }

        /// <summary>
        /// 選課
        /// </summary>
        /// <param name="studentId">學生 Id</param>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpPost]
        public EnrollmentResponse Enroll(int studentId = 0 , int courseId = 0)
        {
            return _enrollmentService.Enroll(studentId, courseId);
        }

        /// <summary>
        /// 更新成績
        /// </summary>
        /// <param name="enrollmentRequest">更新資料</param>
        /// <returns></returns>
        [HttpPut]
        public EnrollmentResponse UpdateGrade([FromBody] EnrollmentRequest enrollmentRequest)
        {
            return _enrollmentService.UpdateGrade(enrollmentRequest);
        }
        /// <summary>
        /// 退選
        /// </summary>
        /// <param name="studentId">學生 Id</param>
        /// <param name="courseId">課程 Id</param>
        /// <returns></returns>
        [HttpDelete]
        public EnrollmentResponse Withdraw(int studentId = 0, int courseId = 0)
        {
            return _enrollmentService.Withdraw(studentId, courseId);
        }
    }
}
