using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private IStudentService _studentSevice;
        public StudentController(IStudentService studentSevice)
        {
            _studentSevice = studentSevice;
        }

        /// <summary>
        /// 取得所有 Student 資料
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public StudentResponse GetAllStudent()
        {
            return _studentSevice.GetAllStudents();
        }

        /// <summary>
        /// 根據 Id 取得 Student 資料
        /// </summary>
        /// <param name="id">學生Id</param>
        /// <returns></returns>
        [HttpGet]
        public StudentResponse GetStudentById(int id = 0)
        {
            return _studentSevice.GetStudentById(id);
        }

        /// <summary>
        /// 新增 Student 資料
        /// </summary>
        /// <param name="studentRequest">Student 資料</param>
        /// <returns></returns>
        [HttpPost]
        public StudentResponse AddStudent([FromBody] StudentRequest studentRequest)
        {
            return _studentSevice.AddStudent(studentRequest);
        }

        /// <summary>
        /// 更新 Student 資料
        /// </summary>
        /// <param name="id">學生 Id</param>
        /// <param name="studentRequest">學生更新項目</param>
        /// <returns></returns>
        [HttpPut]
        public StudentResponse UpdateStudent([FromBody] StudentRequest studentRequest, int id = 0)
        {
            return _studentSevice.UpdateStudent(id, studentRequest);
        }

        /// <summary>
        /// 刪除 Student 資料
        /// </summary>
        /// <param name="id">學生 Id</param>
        /// <returns></returns>
        [HttpDelete]
        public StudentResponse DeleteStudent(int id = 0)
        {
            return _studentSevice.DeleteStudent(id);
        }
    }
}
