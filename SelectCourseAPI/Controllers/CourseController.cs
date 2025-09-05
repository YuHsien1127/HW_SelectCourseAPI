using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// 取得 Course 全部資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public CourseResponse GetAllCourses()
        {
            return _courseService.GetAllCourses();
        }

        /// <summary>
        /// 根據 Id 取得 Course 資料
        /// </summary>
        /// <param name="id">課程 Id</param>
        /// <returns></returns>
        [HttpGet]
        public CourseResponse GetCourseById(int id = 0)
        {
            return _courseService.GetCourseById(id);
        }

        /// <summary>
        /// 新增 Course 資料
        /// </summary>
        /// <param name="courseRequest">Course 資料</param>
        /// <returns></returns>
        [HttpPost]
        public CourseResponse AddCourse([FromBody] CourseRequest courseRequest)
        {
            return _courseService.AddCourse(courseRequest);
        }

        /// <summary>
        /// 更新 Course 資料
        /// </summary>
        /// <param name="courseRequest">Course 資料</param>
        /// <param name="id">課程 Id</param>
        /// <returns></returns>
        [HttpPut]
        public CourseResponse UpdateCourse([FromBody] CourseRequest courseRequest, int id = 0)
        {
            return _courseService.UpdateCourse(id, courseRequest);
        }

        /// <summary>
        /// 刪除 Course 資料
        /// </summary>
        /// <param name="id">課程 Id</param>
        /// <returns></returns>
        [HttpDelete]
        public CourseResponse DeleteCourse(int id = 0)
        {
            return _courseService.DeleteCourse(id);
        }
    }
}
