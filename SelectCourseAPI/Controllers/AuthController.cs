using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;

namespace SelectCourseAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStudentRepository _studentRepository;
        public AuthController(IAuthService authService, IStudentRepository studentRepository)
        {
            _authService = authService;
            _studentRepository = studentRepository;
        }
        /// <summary>
        /// 登入，取得 token
        /// </summary>
        /// <param name="loginRequest">登入資料（Email/Password）</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            var student = _studentRepository.GetStudentByEmail(loginRequest.Email);
            if (student != null && student.Password == loginRequest.Password && student.IsActive == true)
            {
                var token = _authService.GenerateJwtToken(student.Email, student.Role);
                return Ok("Bearer " + token);
            }
            return Unauthorized("帳號或密碼錯誤");
        }
    }
}
