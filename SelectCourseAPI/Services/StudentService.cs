using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using System.Text.RegularExpressions;
using X.PagedList.Extensions;

namespace SelectCourseAPI.Services
{
    public class StudentService : IStudentService
    {
        private readonly SelectCourseContext _context;
        private readonly ILogger<StudentService> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        public StudentService(SelectCourseContext context, ILogger<StudentService> logger, IStudentRepository studentRepoaitory, IEnrollmentRepository enrollmentRepository)
        {
            _context = context;
            _logger = logger;
            _studentRepository = studentRepoaitory;
            _enrollmentRepository = enrollmentRepository;
        }

        public StudentResponse GetAllStudents(int page, int pageSize)
        {
            _logger.LogTrace("【Trace】進入GetAllStudent");
            StudentResponse response = new StudentResponse();

            var students = _studentRepository.GetAllStudents().Where(i => i.IsActive == true)
                .Select(x => new StudentDto
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    Email = x.Email
                });
            _logger.LogDebug("【Debug】取得Student數量：{students.Count()}", students.Count());

            var pagedList = students.ToPagedList(page, pageSize);
            response.Students = pagedList.ToList();
            response.PageCount = pagedList.PageCount;
            response.TotalCount = pagedList.TotalItemCount;
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetAllStudent");
            return response;
        }
        public StudentResponse GetStudentById(int id)
        {
            _logger.LogTrace("【Trace】進入GetStudentById");
            StudentResponse response = new StudentResponse();
            if (id == 0)
            {
                _logger.LogWarning("【Warning】Id為空");
                response.Success = false;
                response.Message = "Id為空";
                return response;
            }
            var student = _studentRepository.GetStudentById(id);
            if (student == null)
            {
                _logger.LogWarning("【Warning】無此Id（{Id}）學生", id);
                response.Success = false;
                response.Message = "無此Id學生";
                return response;
            }
            if(student.IsActive == false)
            {
                _logger.LogWarning("【Warning】此Id（{Id}）學生已停用", id);
                response.Success = false;
                response.Message = "此Id學生已停用";
                return response;
            }
            var s = new StudentDto()
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email
            };
            response.Students = new List<StudentDto> { s };
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetStudentById");
            return response;
        }
        /*
         * 新增學生
         * 1. 驗證必瑱（FirstName/LastName/Email 不能為空）
         * 2. 驗證Email格式（ex. abc@example.com） 
         * 3. 驗證Email是否已存在
         */
        public StudentResponse AddStudent(StudentRequest studentRequest)
        {
            _logger.LogTrace("【Trace】進入AddStudent");
            StudentResponse response = new StudentResponse();

            try
            {
                if (studentRequest == null)
                {
                    _logger.LogWarning("【Warning】新增Student資料為空");
                    response.Success = false;
                    response.Message = "新增Student資料為空";
                    return response;
                }
                else
                {
                    // 驗證必填
                    if (string.IsNullOrEmpty(studentRequest.FirstName) || string.IsNullOrEmpty(studentRequest.LastName) || string.IsNullOrEmpty(studentRequest.Email))
                    {
                        _logger.LogWarning("【Warning】必填欄位不能為空");
                        response.Success = false;
                        response.Message = "必填欄位不能為空";
                        return response;
                    }
                }
                /* Regex.IsMatch 正規畫用法，檢查指定的字串是否符合某個正則表達式模式
                 * 驗證 Email 格式
                 * ^（開頭）
                 * [^@\s]（不能是 @ 或空白的字元，至少一個）
                 * ([^.@\s]+\.)+ （一個或多個「子域名 + .」，例如 gmail.、co.、com.）
                 * [^.@\s]+$ （最後的 TLD，不允許再有 .）
                 */
                if (!Regex.IsMatch(studentRequest.Email, @"^[^@\s]+@([^@\s]+\.)+[^@\s]+$"))
                {
                    _logger.LogWarning("【Warning】Email（{studentRequest.Email}）格式不正確", studentRequest.Email);
                    response.Success = false;
                    response.Message = "Email格式不正確";
                    return response;
                }
                // 檢查 Email 是否已存在
                var existEmail = _studentRepository.GetStudentByEmail(studentRequest.Email);
                if (existEmail != null)
                {
                    _logger.LogWarning("【Warning】Email（{studentRequest.Email}）已存在", studentRequest.Email);
                    response.Success = false;
                    response.Message = "Email已存在";
                    return response;
                }
                var student = new Student
                {
                    FirstName = studentRequest.FirstName,
                    LastName = studentRequest.LastName,
                    Email = studentRequest.Email,
                    CreatedAt = DateTime.Now
                };
                _studentRepository.AddStudent(student);
                int count = _context.SaveChanges();
                var s = new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    Email = student.Email
                };
                if (count > 0)
                {
                    _logger.LogInformation("【Info】新增成功（Id：{student.Id}）", student.Id); // log
                    response.Students = new List<StudentDto> { s };
                    response.Success = true;
                    response.Message = "新增成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】新增失敗");
                    response.Success = false;
                    response.Message = "新增失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】新增發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "新增發生錯誤";
            }
            _logger.LogTrace("【Trace】離開AddStudent");
            return response;
        }
        public StudentResponse UpdateStudent(int id, StudentRequest studentRequest)
        {
            _logger.LogTrace("【Trace】進入UpdateStudent");
            StudentResponse response = new StudentResponse();

            try
            {
                if (id == 0 || studentRequest == null)
                {
                    _logger.LogWarning("【Warning】Id或更新項目為空");
                    response.Success = false;
                    response.Message = "Id或更新項目為空";
                    return response;
                }
                var existStudent = _studentRepository.GetStudentById(id);
                if (existStudent == null)
                {
                    _logger.LogWarning("【Warning】此Id（{id}）的Student資料為空", id); //log
                    response.Success = false;
                    response.Message = "此Id的Student資料為空";
                    return response;
                }
                if (existStudent.IsActive == false)
                {
                    _logger.LogWarning("【Warning】此Id（{Id}）學生已停用", id);
                    response.Success = false;
                    response.Message = "此Id學生已停用";
                    return response;
                }
                existStudent.FirstName = string.IsNullOrEmpty(studentRequest.FirstName) ? existStudent.FirstName : studentRequest.FirstName;
                existStudent.LastName = string.IsNullOrEmpty(studentRequest.LastName) ? existStudent.LastName : studentRequest.LastName;
                existStudent.Email = string.IsNullOrEmpty(studentRequest.Email) ? existStudent.Email : studentRequest.Email;
                existStudent.UpdatedAt = DateTime.Now;
                _studentRepository.UpdateStudent(existStudent);
                int count = _context.SaveChanges();
                var s = new StudentDto
                {
                    Id = id,
                    FirstName = existStudent.FirstName,
                    LastName = existStudent.LastName,
                    Email = existStudent.Email
                };
                if (count > 0)
                {
                    _logger.LogInformation("【Info】更新成功（Id：{id}）", id); // log
                    response.Students = new List<StudentDto> { s };
                    response.Success = true;
                    response.Message = "更新成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】更新失敗");
                    response.Success = false;
                    response.Message = "更新失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】更新發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "更新發生錯誤";
            }
            _logger.LogTrace("【Trace】離開UpdateStudent");
            return response;
        }
        public StudentResponse DeleteStudent(int id)
        {
            _logger.LogTrace("【Trace】進入DeleteStudent");
            StudentResponse response = new StudentResponse();

            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("【Warning】Id為空");
                    response.Success = false;
                    response.Message = "Id為空";
                    return response;
                }
                var student = _studentRepository.GetStudentById(id);
                if (student == null)
                {
                    _logger.LogWarning("【Warning】無此Id（{Id}）學生", id);
                    response.Success = false;
                    response.Message = "無此Id學生";
                    return response;
                }
                _logger.LogDebug("【Debug】準備刪除Student資料（Id ：{student.Id}）", student.Id);
                var enrollmentCount = _enrollmentRepository.GetAllEnrollments().Where(c => c.StudentId == id && c.Status == "A").Count();
                _logger.LogDebug("【Debug】enrollment資料（Count ：{enrollmentCount}）", enrollmentCount);
                if (enrollmentCount > 0)
                {
                    _logger.LogWarning("【Warning】還有課程，無法刪除");
                    response.Success = false;
                    response.Message = "還有課程，無法刪除";
                    return response;
                }
                student.IsActive = false;
                student.UpdatedAt = DateTime.Now;
                _studentRepository.UpdateStudent(student);
                int count = _context.SaveChanges();
                if (count > 0)
                {
                    _logger.LogInformation("【Info】刪除成功（Id：{student.Id}）", student.Id); // log
                    response.Success = true;
                    response.Message = "刪除成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】刪除失敗");
                    response.Success = false;
                    response.Message = "刪除失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】刪除發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "刪除發生錯誤";
            }
            _logger.LogTrace("【Trace】離開DeleteStudent");
            return response;
        }

    }
}
