using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;

namespace SelectCourseAPI.Services
{
    public class StudentService : IStudentService
    {
        private readonly SelectCourseContext _context;
        private readonly ILogger<StudentService> _logger;
        private readonly IStudentRepository _studentRepoaitory;
        public StudentService(SelectCourseContext context, ILogger<StudentService> logger, IStudentRepository studentRepoaitory)
        {
            _context = context;
            _logger = logger;
            _studentRepoaitory = studentRepoaitory;
        }

        public StudentResponse GetAllStudents()
        {
            _logger.LogTrace("【Trace】進入GetAllStudent");
            StudentResponse response = new StudentResponse();

            var students = _studentRepoaitory.GetAllStudents();
            _logger.LogDebug("【Debug】取得Student數量：{students.Count()}", students.Count());
            var s = students.Select(x => new StudentDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email
            });
            response.Students = s.ToList();
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
            var student = _studentRepoaitory.GetStudentById(id);
            if (student == null)
            {
                _logger.LogWarning("【Warning】無此Id（{Id}）學生", id);
                response.Success = false;
                response.Message = "無此Id學生";
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
                var student = new Student
                {
                    FirstName = studentRequest.FirstName,
                    LastName = studentRequest.LastName,
                    Email = studentRequest.Email,
                    CreatedAt = DateTime.Now
                };
                _studentRepoaitory.AddStudent(student);
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
                var existStudent = _studentRepoaitory.GetStudentById(id);
                if (existStudent == null)
                {
                    _logger.LogWarning("【Warning】此Id（{id}）的Student資料為空", id); //log
                    response.Success = false;
                    response.Message = "Student資料為空";
                    return response;
                }
                existStudent.FirstName = studentRequest.FirstName == "" ? existStudent.FirstName : studentRequest.FirstName;
                existStudent.LastName = studentRequest.LastName == "" ? existStudent.LastName : studentRequest.LastName;
                existStudent.Email = studentRequest.Email == "" ? existStudent.Email : studentRequest.Email;
                existStudent.UpdatedAt = DateTime.Now;
                _studentRepoaitory.UpdateStudent(existStudent);
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
                var student = _studentRepoaitory.GetStudentById(id);
                if (student == null)
                {
                    _logger.LogWarning("【Warning】無此Id（{Id}）學生", id);
                    response.Success = false;
                    response.Message = "無此Id學生";
                    return response;
                }
                _logger.LogDebug("【Debug】準備刪除Student資料（Id ：{student.Id}）", student.Id);
                _studentRepoaitory.DeleteStudent(student);
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
