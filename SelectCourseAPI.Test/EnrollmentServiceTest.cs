using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SelectCourseAPI.Test
{
    [TestFixture]
    public class EnrollmentServiceTest
    {
        private SelectCourseContext _context;
        private EnrollmentService _enrollmentService;
        private Mock<ILogger<EnrollmentService>> _mockLogger;
        private Mock<IStudentRepository> _mockStudentRepository;
        private Mock<ICourseRepository> _mockCourseRepository;
        private Mock<IEnrollmentRepository> _mockEnrollmentRepository;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SelectCourseContext>()
                .UseInMemoryDatabase(databaseName: "TestEnrollmentDB")
                .Options;
            _context = new SelectCourseContext(options);
            // 清掉舊資料，避免測試間互相干擾
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed Student
            _context.Students.AddRange(
                new Student { Id = 1, FirstName = "A", LastName = "B", Email = "ab@example.com", Password = "abPassword", Role = "user", IsActive = true },
                new Student { Id = 2, FirstName = "C", LastName = "D", Email = "cd@example.com", Password = "cdPassword", Role = "user", IsActive = false },
                new Student { Id = 3, FirstName = "E", LastName = "F", Email = "ef@example.com", Password = "efPassword", Role = "user", IsActive = true },
                new Student { Id = 4, FirstName = "G", LastName = "H", Email = "gh@example.com", Password = "ghPassword", Role = "user", IsActive = true },
                new Student { Id = 5, FirstName = "I", LastName = "J", Email = "ij@example.com", Password = "ijPassword", Role = "user", IsActive = true }
            );
            // Seed Course
            _context.Courses.AddRange(
                new Course { Id = 1, Code = "C001", Title = "Math", Credits = 3, IsActive = true, IsDel = false },
                new Course { Id = 2, Code = "C002", Title = "History", Credits = 2, IsActive = false, IsDel = false },
                new Course { Id = 3, Code = "C003", Title = "English", Credits = 3, IsActive = true, IsDel = false },
                new Course { Id = 4, Code = "C004", Title = "Chinese", Credits = 3, IsActive = true, IsDel = false }
            );
            // Seed Enrollment
            _context.Enrollments.AddRange(
                new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Grade = 90, LetterGrade = "A", GradePoint = 4m, RowVersion = 0, Status = "A" },
                new Enrollment { Id = 2, StudentId = 1, CourseId = 3, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "W" },
                new Enrollment { Id = 3, StudentId = 5, CourseId = 4, Grade = 80, LetterGrade = "B", GradePoint = 3m, RowVersion = 0, Status = "A" },
                new Enrollment { Id = 4, StudentId = 3, CourseId = 3, Grade = 75, LetterGrade = "C", GradePoint = 2m, RowVersion = 0, Status = "C" },
                new Enrollment { Id = 5, StudentId = 4, CourseId = 1, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "A" }
            );
            _context.SaveChanges();

            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();

            // Repository 模擬回傳資料
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>()))
                .Returns((int id) => _context.Courses.FirstOrDefault(c => c.Id == id));

            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            _mockStudentRepository.Setup(r => r.GetStudentByEmail(It.IsAny<string>()))
                .Returns((string email) => _context.Students.FirstOrDefault(s => s.Email == email));

            _mockEnrollmentRepository.Setup(r => r.GetEnrollmentById(It.IsAny<int>(), It.IsAny<int>()))
                .Returns((int sId, int cId) => _context.Enrollments.FirstOrDefault(e => e.StudentId == sId && e.CourseId == cId));
            _mockEnrollmentRepository.Setup(r => r.AddEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e =>
                {
                    e.CreatedAt = DateTime.Now;
                    e.Status = "A";
                    _context.Enrollments.Add(e);
                });
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));

            _mockLogger = new Mock<ILogger<EnrollmentService>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _enrollmentService = new EnrollmentService(
                _context,
                _mockLogger.Object,
                _mockEnrollmentRepository.Object,
                _mockCourseRepository.Object,
                _mockStudentRepository.Object,
                _mockHttpContextAccessor.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllEnrollments
        [Test] // 測試 GetAllEnrollments
        public void GetAllEnrollment_ReturnSuccess()
        {
            _mockEnrollmentRepository.Setup(r => r.GetAllEnrollments()).Returns(_context.Enrollments.AsQueryable());
            var result = _enrollmentService.GetAllEnrollments(1, 10);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Enrollments.Count, Is.EqualTo(4));
        }
        #endregion
        #region GetEnrollmentById
        [Test] // 測試 GetEnrollmentById => 存在
        public void GetEnrollmentById_ReturnSuccess()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 1;            
            var result = _enrollmentService.GetEnrollmentById(courseId);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Enrollments[0].Grade, Is.EqualTo(90));
        }
        [Test] // 測試 GetEnrollmentById => courseId == 0
        public void GetEnrollmentById_ZeroCourseId_ReturnFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 0;
            var result = _enrollmentService.GetEnrollmentById(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("StudentId或CourseId為空"));
        }
        [Test] // 測試 GetEnrollmentById => Enrollment 不存在
        public void GetEnrollmentById_NoExisting_ReturnFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 4;
            var result = _enrollmentService.GetEnrollmentById(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("無此選課資料"));
        }
        [Test] // 測試 GetEnrollmentById => 已退選
        public void GetEnrollmentById_Withdraw_ReturnFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 3;            
            var result = _enrollmentService.GetEnrollmentById(courseId);
            //Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("已退選"));
        }
        #endregion

        #region Enroll
        [Test] // 測試 Enroll => 成功
        public void Enroll_ReturnSuccess()
        {
            string email = "ij@example.com";
            SetupHttpContext(email);
            int courseId = 3;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("選課成功"));
            Assert.That(result.Enrollments.Count, Is.EqualTo(1));
        }
        [Test] // 測試 Enroll => HasEnrollment && Enrollment.Status == "W" 成功
        public void Enroll_HasEnrollmentIsWithdraw_ReturnSuccess()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 3;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("該課程已重新選課成功"));
            var studentId = _context.Students.First(s => s.Email == email).Id;
            var enrollment = _context.Enrollments
               .FirstOrDefault(e => e.StudentId == studentId && e.CourseId == courseId);
            Assert.That(enrollment.Status, Is.EqualTo("A"));
        }        
        [Test] // 測試 Enroll => courseId == 0
        public void Enroll_ZeroCourseId_ReturnFail()
        {
            string email = "ij@example.com";
            SetupHttpContext(email);
            int courseId = 0;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Id為空"));
        }        

        [Test] // 測試 Enroll => Course null
        public void Enroll_NullCourse_ReturnFail()
        {
            string email = "ij@example.com";
            SetupHttpContext(email);
            int courseId = 99;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("課程不存在或已停用"));
        }
        [Test] // 測試 Enroll => Course.IsActice == false
        public void Enroll_NoIsActiceCourse_ReturnFail()
        {
            string email = "ij@example.com";
            SetupHttpContext(email);
            int courseId = 2;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("課程不存在或已停用"));
        }
        [Test] // 測試 Enroll => 已選課
        public void Enroll_HasCourse_ReturnFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 1;
            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("已選過該課程"));
        }
        [Test] // 測試 AddCourse => try catch
        public void Enroll_DbSaveFailure_ReturnFail()
        {
            string email = "ij@example.com";
            SetupHttpContext(email);
            int courseId = 1;
            _mockEnrollmentRepository.Setup(r => r.AddEnrollment(It.IsAny<Enrollment>()))
                .Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var result = _enrollmentService.Enroll(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("選課發生錯誤"));
        }
        #endregion

        #region UpdateGade
        [Test] // 測試 UpdateGaded => 成功
        public void UpdateGrade_ReturrnSuccess()
        {
            var enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 1, Grade = 67 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("更新成績成功"));
            var enrollment = _context.Enrollments
                .FirstOrDefault(e => e.StudentId == enrollmentRequest.StudentId && e.CourseId == enrollmentRequest.CourseId);
            Assert.That(enrollment.Grade, Is.EqualTo(67));
            Assert.That(enrollment.RowVersion, Is.EqualTo(1));
        }
        [Test] // 測試 UpdateGaded => EnrollmentRequest null
        public void UpdateGrade_NullRequest_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = null;
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("資料為空"));
        }
        [Test] // 測試 UpdateGaded => StudentId == 0
        public void UpdateGrade_ZeroStudentId_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 0, CourseId = 1, Grade = 67 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("資料不完整"));
        }
        [Test] // 測試 UpdateGaded => CourseId == 0
        public void UpdateGrade_ZeroCourseId_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 0, Grade = 67 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("資料不完整"));
        }
        [Test] // 測試 UpdateGaded => Grade null
        public void UpdateGrade_ZeroGrade_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 1, Grade = null };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("資料不完整"));
        }
        [Test] // 測試 UpdateGaded => Enrollment 不存在
        public void UpdateGrade_NoExisting_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 2, Grade = 67 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("資料不存在"));
        }
        [Test] // 測試 UpdateGaded => 已退選
        public void UpdateGrade_IsWithdraw_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 3, Grade = 76 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("已退選"));
        }
        [Test] // 測試 UpdateGaded => Grade 超出範圍
        public void UpdateGrade_OutOfRangeGrade_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 1, CourseId = 3, Grade = 101 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("成績超過範圍（0~100）"));
        }
        [Test] // 測試 UpdateGaded => 已結束課程
        public void UpdateGrade_EndCourse_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 3, CourseId = 3, Grade = 76 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Callback<Enrollment>(e => _context.Enrollments.Update(e));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("課程已結束，無法更新成績"));
        }
        [Test] // 測試 UpdateGaded => try catch
        public void UpdateGrade_DbSaveFailure_ReturrnFail()
        {
            EnrollmentRequest enrollmentRequest = new EnrollmentRequest { StudentId = 4, CourseId = 1, Grade = 67 };
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Throws(new DbUpdateException("更新成績發生錯誤"));
            var result = _enrollmentService.UpdateGrade(enrollmentRequest);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("更新成績發生錯誤"));
        }
        #endregion

        #region Withdraw
        [Test] // 測試 Withdraw => 成功
        public void Withdraw_RetunSuccess()
        {
            string email = "gh@example.com";
            SetupHttpContext(email);
            int courseId = 1;

            var result = _enrollmentService.Withdraw(courseId);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("退選成功"));
        }
        [Test] // 測試 Withdraw => CourseId == 0
        public void Withdraw_ZeroCourseId_RetunFail()
        {
            string email = "gh@example.com";
            SetupHttpContext(email);
            int courseId = 0;

            var result = _enrollmentService.Withdraw(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("StudentId或CourseId為空"));
        }
        [Test] // 測試 Withdraw => Enrollment 不存在
        public void Withdraw_NoExisting_RetunFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 2;

            var result = _enrollmentService.Withdraw(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("無此選課資料"));
        }
        [Test] // 測試 Withdraw => HasGrade
        public void Withdraw_HasGrade_RetunFail()
        {
            string email = "ab@example.com";
            SetupHttpContext(email);
            int courseId = 1;

            var result = _enrollmentService.Withdraw(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("已有成績，無法退選"));
        }
        [Test] // 測試 Withdraw => try catchh
        public void Withdraw_DbSaveFailure_RetunFail()
        {
            string email = "gh@example.com";
            SetupHttpContext(email);
            int courseId = 1;
            _mockEnrollmentRepository.Setup(r => r.UpdateEnrollment(It.IsAny<Enrollment>()))
                .Throws(new DbUpdateException("更新成績發生錯誤"));

            var result = _enrollmentService.Withdraw(courseId);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("退選發生錯誤"));
        }
        #endregion

        // 模擬登入使用者的 HttpContext
        private void SetupHttpContext(string email)
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, email) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            var context = new Mock<HttpContext>();
            context.Setup(c => c.User).Returns(principal);

            _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(context.Object);
        }
    }
}
