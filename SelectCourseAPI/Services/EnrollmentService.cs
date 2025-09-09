﻿using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using System.Security.Claims;
using X.PagedList.Extensions;

namespace SelectCourseAPI.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly SelectCourseContext _context;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IHttpContextAccessor _HttpContextAccessor;
        public EnrollmentService(SelectCourseContext context, ILogger<EnrollmentService> logger, IEnrollmentRepository enrollmentRepository
            , ICourseRepository courseRepository, IStudentRepository studentRepository, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _HttpContextAccessor = httpContextAccessor;
        }

        public EnrollmentResponse GetAllEnrollments(int page, int pageSize)
        {
            _logger.LogTrace("【Trace】進入GetAllEnrollments");
            EnrollmentResponse response = new EnrollmentResponse();
            var enrollment = _enrollmentRepository.GetAllEnrollments().Where(s => s.Status == "A" || s.Status == "C")
                .Select(e => new EnrollmentDto
                {
                    StudentId = e.StudentId,
                    Student = new StudentDto
                    {
                        Id = e.StudentId,
                        FirstName = e.Student.FirstName,
                        LastName = e.Student.LastName,
                        Email = e.Student.Email
                    },
                    CourseId = e.CourseId,
                    Courses = new CourseDto
                    {
                        Id = e.CourseId,
                        Code = e.Course.Code,
                        Credits = e.Course.Credits,
                        Title = e.Course.Title
                    },
                    Grade = e.Grade,
                    LetterGrade = e.LetterGrade,
                    GradePoint = e.GradePoint,
                });
            var pagedList = enrollment.ToPagedList(page, pageSize);
            response.Enrollments = pagedList.ToList();
            response.PageCount = pagedList.PageCount;
            response.TotalCount = pagedList.TotalItemCount;
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetAllEnrollments");
            return response;
        }
        public EnrollmentResponse GetEnrollmentById(int courseId)
        {
            _logger.LogTrace("【Trace】進入GetEnrollmentById");
            EnrollmentResponse response = new EnrollmentResponse();
            if (courseId == 0)
            {
                _logger.LogWarning("【Warning】StudentId或CourseId為空");
                response.Success = false;
                response.Message = "StudentId或CourseId為空";
                _logger.LogTrace("【Trace】離開GetEnrollmentById");
                return response;
            }
            var email = _HttpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int studentId = _studentRepository.GetStudentByEmail(email).Id;
            var enrollment = _enrollmentRepository.GetEnrollmentById(studentId, courseId);
            if (enrollment == null)
            {
                _logger.LogWarning("【Warning】無此選課資料");
                response.Success = false;
                response.Message = "無此選課資料";
                _logger.LogTrace("【Trace】離開GetEnrollmentById");
                return response;
            }
            if (enrollment.Status == "W")
            {
                _logger.LogWarning("【Warning】已退選");
                response.Success = false;
                response.Message = "已退選";
                _logger.LogTrace("【Trace】離開GetEnrollmentById");
                return response;
            }
            var e = new EnrollmentDto
            {
                StudentId = enrollment.StudentId,
                Student = new StudentDto
                {
                    Id = enrollment.StudentId,
                    FirstName = enrollment.Student.FirstName,
                    LastName = enrollment.Student.LastName,
                    Email = enrollment.Student.Email
                },
                CourseId = enrollment.CourseId,
                Courses = new CourseDto
                {
                    Id = enrollment.CourseId,
                    Code = enrollment.Course.Code,
                    Credits = enrollment.Course.Credits,
                    Title = enrollment.Course.Title
                },
                Grade = enrollment.Grade,
                LetterGrade = enrollment.LetterGrade,
                GradePoint = enrollment.GradePoint,
            };
            response.Enrollments = new List<EnrollmentDto> { e };
            response.Success = true;
            response.Message = "查詢成功";
            _logger.LogTrace("【Trace】離開GetEnrollmentById");
            return response;
        }

        /* 選課
         * 1. 驗證 Student/Course 存在且 IsActice
         * 2. 檢查是否已選過該課程
         */
        public EnrollmentResponse Enroll(int courseId)
        {
            _logger.LogTrace("【Trace】進入Enroll");
            EnrollmentResponse response = new EnrollmentResponse();

            try
            {
                if (courseId == 0)
                {
                    _logger.LogWarning("【Warning】Id為空");
                    response.Success = false;
                    response.Message = "Id為空";
                    _logger.LogTrace("【Trace】離開Enroll");
                    return response;
                }
                var email = _HttpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int studentId = _studentRepository.GetStudentByEmail(email).Id;                
                var course = _courseRepository.GetCourseById(courseId);
                if (course == null || course.IsActive == false)
                {
                    _logger.LogWarning("【Warning】課程不存在或已停用：{CourseId}", courseId);
                    response.Success = false;
                    response.Message = "課程不存在或已停用";
                    _logger.LogTrace("【Trace】離開Enroll");
                    return response;
                }
                // 檢查是否已選過該課程
                var existEnrollment = _enrollmentRepository.GetEnrollmentById(studentId, courseId);
                if (existEnrollment != null && existEnrollment.Status == "A")
                {
                    _logger.LogWarning("【Warning】學生已選過該課程：StudentId={StudentId}, CourseId={CourseId}", studentId, courseId);
                    response.Success = false;
                    response.Message = "已選過該課程";
                    _logger.LogTrace("【Trace】離開Enroll");
                    return response;
                }
                if (existEnrollment != null && existEnrollment.Status == "W")
                {
                    existEnrollment.Status = "A";
                    existEnrollment.CreatedAt = DateTime.Now;
                    _context.SaveChanges();
                    response.Success = true;
                    response.Message = "該課程已重新選課成功";
                    _logger.LogTrace("【Trace】離開Enroll");
                    return response;
                }
                // 新增選課
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = courseId,
                    CreatedAt = DateTime.Now,
                    Status = "A"
                };
                _enrollmentRepository.AddEnrollment(enrollment);
                int count = _context.SaveChanges();

                if (count > 0)
                {
                    var e = new EnrollmentDto
                    {
                        StudentId = enrollment.StudentId,
                        Student = new StudentDto
                        {
                            Id = studentId,
                            FirstName = enrollment.Student.FirstName,
                            LastName = enrollment.Student.LastName,
                            Email = enrollment.Student.Email
                        },
                        CourseId = enrollment.CourseId,
                        Courses = new CourseDto
                        {
                            Id = courseId,
                            Code = enrollment.Course.Code,
                            Credits = enrollment.Course.Credits,
                            Title = enrollment.Course.Title
                        },
                        Grade = enrollment.Grade,
                        LetterGrade = enrollment.LetterGrade,
                        GradePoint = enrollment.GradePoint
                    };
                    _logger.LogInformation("【Info】選課成功（StudentId/CourseId：{studentId}/{courseId}）", studentId, courseId); // log
                    response.Enrollments = new List<EnrollmentDto> { e };
                    response.Success = true;
                    response.Message = "選課成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】選課失敗");
                    response.Success = false;
                    response.Message = "選課失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】選課發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "選課發生錯誤";
            }
            _logger.LogTrace("【Trace】離開Enroll");
            return response;
        }
        /* 更新成績
         * 1. 課程若不存在 => 回傳404
         * 2. 驗證 Grade 範圍（0~100）
         * 3. 計算 LetterGrade/GradePoint
         * 4. RowVersion 使用 （更新一次 +1）
         */
        public EnrollmentResponse UpdateGrade(EnrollmentRequest enrollmentRequest)
        {
            _logger.LogTrace("【Trace】進入UpdateGrade");
            EnrollmentResponse response = new EnrollmentResponse();

            try
            {
                if (enrollmentRequest == null)
                {
                    _logger.LogWarning("【Warning】資料為空");
                    response.Success = false;
                    response.Message = "資料為空";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }
                if (enrollmentRequest.StudentId == 0 || enrollmentRequest.CourseId == 0 || enrollmentRequest.Grade == null)
                {
                    _logger.LogWarning("【Warning】資料不完整");
                    response.Success = false;
                    response.Message = "資料不完整";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }
                if (enrollmentRequest.Grade < 0 || enrollmentRequest.Grade > 100)
                {
                    _logger.LogWarning("【Warning】成績（{enrollment.Grade}）超過範圍（0~100）", enrollmentRequest.Grade);
                    response.Success = false;
                    response.Message = "成績超過範圍（0~100）";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }
                var enrollment = _enrollmentRepository.GetEnrollmentById(enrollmentRequest.StudentId, enrollmentRequest.CourseId);
                if (enrollment == null)
                {
                    _logger.LogWarning("【Warning】資料不存在");
                    response.Success = false;
                    response.Message = "資料不存在";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }
                if (enrollment.Status == "W")
                {
                    _logger.LogWarning("【Warning】已退選");
                    response.Success = false;
                    response.Message = "已退選";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }
                if(enrollment.Status == "C")
                {
                    _logger.LogWarning("【Warning】課程已結束，無法更新成績");
                    response.Success = false;
                    response.Message = "課程已結束，無法更新成績";
                    _logger.LogTrace("【Trace】離開UpdateGrade");
                    return response;
                }                        
                if (enrollment.Grade != null)
                    enrollment.RowVersion += 1;
                enrollment.Grade = enrollmentRequest.Grade;
                enrollment.LetterGrade = GetLetterGrade((int)enrollmentRequest.Grade);
                enrollment.GradePoint = GetGradePoint(enrollment.LetterGrade);
                enrollment.UpdatedAt = DateTime.Now;
                _enrollmentRepository.UpdateEnrollment(enrollment);
                int count = _context.SaveChanges();
                if (count > 0)
                {
                    var e = new EnrollmentDto
                    {
                        StudentId = enrollment.StudentId,
                        Student = new StudentDto
                        {
                            Id = enrollment.StudentId,
                            FirstName = enrollment.Student.FirstName,
                            LastName = enrollment.Student.LastName,
                            Email = enrollment.Student.Email
                        },
                        CourseId = enrollment.CourseId,
                        Courses = new CourseDto
                        {
                            Id = enrollment.CourseId,
                            Code = enrollment.Course.Code,
                            Credits = enrollment.Course.Credits,
                            Title = enrollment.Course.Title
                        },
                        Grade = enrollment.Grade,
                        LetterGrade = enrollment.LetterGrade,
                        GradePoint = enrollment.GradePoint,
                    };
                    _logger.LogInformation("【Info】更新成績成功（StudentId/CourseId：{enrollment.StudentId}/{enrollment.CourseId}）", enrollment.StudentId, enrollment.CourseId); // log
                    response.Enrollments = new List<EnrollmentDto> { e };
                    response.Success = true;
                    response.Message = "更新成績成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】更新成績失敗");
                    response.Success = false;
                    response.Message = "更新成績失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】更新成績發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "更新成績發生錯誤";
            }
            _logger.LogTrace("【Trace】離開UpdateGrade");
            return response;
        }
        /* 退選
         * 1. 課程若不存在 => 回傳404
         * 2. 退選時若有成績則無法退選
         * 3. 退選時無成績，則將 Status 設定 W
         */
        public EnrollmentResponse Withdraw(int courseId)
        {
            _logger.LogTrace("【Trace】進入Withdraw");
            EnrollmentResponse response = new EnrollmentResponse();

            try
            {
                if (courseId == 0)
                {
                    _logger.LogWarning("【Warning】StudentId或CourseId為空");
                    response.Success = false;
                    response.Message = "StudentId或CourseId為空";
                    _logger.LogTrace("【Trace】離開Withdraw");
                    return response;
                }
                var email = _HttpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int studentId = _studentRepository.GetStudentByEmail(email).Id;
                var enrollment = _enrollmentRepository.GetEnrollmentById(studentId, courseId);
                if (enrollment == null)
                {
                    _logger.LogWarning("【Warning】無此選課資料");
                    response.Success = false;
                    response.Message = "無此選課資料";
                    _logger.LogTrace("【Trace】離開Withdraw");
                    return response;
                }
                if (enrollment.Grade.HasValue)
                {
                    _logger.LogWarning("【Warning】已有成績（{enrollment.Grade}），無法退選", enrollment.Grade);
                    response.Success = false;
                    response.Message = "已有成績，無法退選";
                    _logger.LogTrace("【Trace】離開Withdraw");
                    return response;
                }
                enrollment.Status = "W";
                enrollment.UpdatedAt = DateTime.Now;
                _enrollmentRepository.UpdateEnrollment(enrollment);
                int count = _context.SaveChanges();
                if (count > 0)
                {
                    _logger.LogInformation("【Info】退選成功（StudentId/CourseId：{enrollment.StudentId}/{enrollment.CourseId}）", enrollment.StudentId, enrollment.CourseId); // log
                    response.Success = true;
                    response.Message = "退選成功";
                }
                else
                {
                    _logger.LogWarning("【Warning】退選失敗");
                    response.Success = false;
                    response.Message = "退選失敗";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【Error】退選發生錯誤"); //log 嚴重錯誤
                response.Success = false;
                response.Message = "退選發生錯誤";
            }
            _logger.LogTrace("【Trace】離開Withdraw");
            return response;
        }

        // 計算 LetterGrade
        private string GetLetterGrade(int grade)
        {
            if (grade >= 90) return "A";
            if (grade >= 80) return "B";
            if (grade >= 70) return "C";
            if (grade >= 60) return "D";
            return "F";
        }
        // 計算 GradePoint
        private decimal GetGradePoint(string letterGrade)
        {
            decimal point = 0;
            switch (letterGrade)
            {
                case "A":
                    point = 4m;
                    break;
                case "B":
                    point = 3m;
                    break;
                case "C":
                    point = 2m;
                    break;
                case "D":
                    point = 1m;
                    break;
                default:
                    point = 0m;
                    break;
            }
            return point;
        }
    }
}
