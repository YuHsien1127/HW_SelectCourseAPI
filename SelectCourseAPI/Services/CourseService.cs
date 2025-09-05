using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;

namespace SelectCourseAPI.Services
{
    public class CourseService : ICourseService
    {
        private readonly SelectCourseContext _context;
        private readonly ILogger<CourseService> _logger;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        public CourseService(SelectCourseContext context, ILogger<CourseService> logger, ICourseRepository courseRepository, IEnrollmentRepository enrollmentRepository)
        {
            _context = context;
            _logger = logger;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public CourseResponse GetAllCourses()
        {
            _logger.LogTrace("【Trace】進入GetAllCourse");
            CourseResponse response = new CourseResponse();

            var courses = _courseRepository.GetAllCourses().Where(i => i.IsActive == true)
                .Select(x => new CourseDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Credits = x.Credits,
                    Title = x.Title
                });
            _logger.LogDebug("【Debug】取得Course數量：{courses.Count()}", courses.Count());
            response.Courses = courses.ToList();
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetAllCourse");
            return response;
        }

        public CourseResponse GetCourseById(int id)
        {
            _logger.LogTrace("【Trace】進入GetCourseById");
            CourseResponse response = new CourseResponse();
            if (id == 0)
            {
                _logger.LogWarning("【Warning】Id為空");
                response.Success = false;
                response.Message = "Id為空";
                return response;
            }
            var course = _courseRepository.GetCourseById(id);
            if (course == null)
            {
                _logger.LogWarning("【Warning】無此Id（{Id}）課程", id);
                response.Success = false;
                response.Message = "無此Id課程";
                return response;
            }
            if (course.IsActive == false)
            {
                _logger.LogWarning("【Warning】此Id（{Id}）課程已停用", id);
                response.Success = false;
                response.Message = "此Id課程已停用";
                return response;
            }
            var c = new CourseDto()
            {
                Id = course.Id,
                Code = course.Code,
                Credits = course.Credits,
                Title = course.Title
            };
            response.Courses = new List<CourseDto> { c };
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetCourseById");
            return response;
        }

        public CourseResponse AddCourse(CourseRequest courseRequest)
        {
            _logger.LogTrace("【Trace】進入AddCourse");
            CourseResponse response = new CourseResponse();

            try
            {
                if (courseRequest == null)
                {
                    _logger.LogWarning("【Warning】新增Course資料為空");
                    response.Success = false;
                    response.Message = "新增Course資料為空";
                    return response;
                }
                var existCourse = _courseRepository.GetCourseByCode(courseRequest.Code);
                if (existCourse != null)
                {
                    _logger.LogWarning("【Warning】課程代碼（{courseRequest.Code}）已存在", courseRequest.Code);
                    response.Success = false;
                    response.Message = "課程代碼已存在";
                    return response;
                }
                var course = new Course
                {
                    Code = courseRequest.Code,
                    Title = courseRequest.Title,
                    Credits = courseRequest.Credits,
                    CreatedAt = DateTime.Now
                };
                _courseRepository.AddCourse(course);
                int count = _context.SaveChanges();
                var c = new CourseDto
                {
                    Id = course.Id,
                    Code = course.Code,
                    Title = course.Title,
                    Credits = course.Credits
                };
                if (count > 0)
                {
                    _logger.LogInformation("【Info】新增成功（Id：{course.Id}）", course.Id); // log
                    response.Courses = new List<CourseDto> { c };
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
            _logger.LogTrace("【Trace】離開AddCourse");
            return response;
        }

        public CourseResponse DeleteCourse(int id)
        {
            _logger.LogTrace("【Trace】進入DeleteCourse");
            CourseResponse response = new CourseResponse();

            try
            {
                if (id == 0)
                {
                    _logger.LogWarning("【Warning】Id為空");
                    response.Success = false;
                    response.Message = "Id為空";
                    return response;
                }
                var course = _courseRepository.GetCourseById(id);
                if (course == null)
                {
                    _logger.LogWarning("【Warning】無此Id（{Id}）課程", id);
                    response.Success = false;
                    response.Message = "無此Id課程";
                    return response;
                }
                _logger.LogDebug("【Debug】準備刪除Course資料（Id ：{course.Id}）", course.Id);
                var enrollmentCount = _enrollmentRepository.GetAllEnrollments().Where(c => c.CourseId == id && c.Status == "A ").Count();
                _logger.LogDebug("【Debug】enrollment資料（Count ：{enrollmentCount}）", enrollmentCount);
                if(enrollmentCount > 0)
                {
                    _logger.LogWarning("【Warning】有被選課，無法刪除");
                    response.Success = false;
                    response.Message = "有被選課，無法刪除";
                    return response;
                }
                course.IsActive = false;
                course.UpdatedAt = DateTime.Now;
                _courseRepository.UpdateCourse(course);
                int count = _context.SaveChanges();
                if (count > 0)
                {
                    _logger.LogInformation("【Info】刪除成功（Id：{course.Id}）", course.Id); // log
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
            _logger.LogTrace("【Trace】離開DeleteCourse");
            return response;
        }

        public CourseResponse UpdateCourse(int id, CourseRequest courseRequest)
        {
            _logger.LogTrace("【Trace】進入UpdateCourse");
            CourseResponse response = new CourseResponse();

            try
            {
                if (id == 0 || courseRequest == null)
                {
                    _logger.LogWarning("【Warning】Id或更新項目為空");
                    response.Success = false;
                    response.Message = "Id或更新項目為空";
                    return response;
                }
                var existCourse = _courseRepository.GetCourseById(id);
                if (existCourse == null)
                {
                    _logger.LogWarning("【Warning】此Id（{id}）的Course資料為空", id); //log
                    response.Success = false;
                    response.Message = "Course資料為空";
                    return response;
                }
                if (existCourse.IsActive == false)
                {
                    _logger.LogWarning("【Warning】此Id（{Id}）課程已停用", id);
                    response.Success = false;
                    response.Message = "此Id課程已停用";
                    return response;
                }
                existCourse.Code = courseRequest.Code == "" ? existCourse.Code : courseRequest.Code;
                existCourse.Title = courseRequest.Title == "" ? existCourse.Title : courseRequest.Title;
                existCourse.Credits = courseRequest.Credits == 0 ? existCourse.Credits : courseRequest.Credits;
                existCourse.UpdatedAt = DateTime.Now;
                _courseRepository.UpdateCourse(existCourse);
                int count = _context.SaveChanges();
                var c = new CourseDto
                {
                    Id = id,
                    Code = existCourse.Code,
                    Title = existCourse.Title,
                    Credits = existCourse.Credits
                };
                if (count > 0)
                {
                    _logger.LogInformation("【Info】更新成功（Id：{id}）", id); // log
                    response.Courses = new List<CourseDto> { c };
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
            _logger.LogTrace("【Trace】離開UpdateCourse");
            return response;
        }
    }
}
