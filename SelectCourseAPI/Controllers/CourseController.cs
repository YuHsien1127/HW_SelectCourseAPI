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
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">幾筆</param>
        /// <returns></returns>
        [HttpGet]
        public CourseResponse GetAllCourses(int page = 1, int pageSize = 10)
        {
            return _courseService.GetAllCourses(page, pageSize);
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
        [Authorize(Roles = "admit")]
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
        [Authorize(Roles = "admit")]
        public CourseResponse UpdateCourse([FromBody] CourseRequest courseRequest, int id = 0)
        {
            return _courseService.UpdateCourse(id, courseRequest);
        }

        /// <summary>
        /// 關閉課程（IsActice）
        /// </summary>
        /// <param name="id">課程 Id</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = "admit")]
        public CourseResponse CloseCourse(int id = 0)
        {
            return _courseService.StopCourse(id);
        }
        /// <summary>
        /// 刪除課程（IsDel）
        /// </summary>
        /// <param name="id">課程 Id</param>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = "admit")]
        public CourseResponse DeleteCourse(int id = 0)
        {
            return _courseService.DeleteCourse(id);
        }
    }
}
