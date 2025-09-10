using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "admin, user")]
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
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">幾筆</param>
        /// <returns></returns>
        [HttpGet]
        public StudentResponse GetAllStudent(int page = 1, int pageSize = 10)
        {
            return _studentSevice.GetAllStudents(page, pageSize);
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
        [AllowAnonymous]
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
