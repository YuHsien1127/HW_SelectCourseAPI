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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SelectCourseAPI.Test
{
    [TestFixture]
    public class CourseServiceTest
    {
        private Mock<ICourseRepository> _mockCourseRepository;
        private Mock<IEnrollmentRepository> _mockEnrollmentRepository;
        private Mock<ILogger<CourseService>> _mockLogger;
        private CourseService _courseService;
        private SelectCourseContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SelectCourseContext>()
                .UseInMemoryDatabase(databaseName: "TestCourseDB").Options;
            _context = new SelectCourseContext(options);
            // 清掉舊資料，避免測試間互相干擾
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed 初始資料
            _context.Courses.AddRange(
                new Course { Id = 1, Code = "A", Title = "B", Credits = 3, IsActive = true },
                new Course { Id = 2, Code = "C", Title = "D", Credits = 3, IsActive = false },
                new Course { Id = 3, Code = "E", Title = "F", Credits = 3, IsActive = true },
                new Course { Id = 4, Code = "G", Title = "H", Credits = 3, IsActive = true }
            );
            // Seed Enrollment
            _context.Enrollments.AddRange(
                new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Grade = 90, LetterGrade = "A", GradePoint = 4m, RowVersion = 0, Status = "A" },
                new Enrollment { Id = 2, StudentId = 1, CourseId = 3, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "W" },
                new Enrollment { Id = 3, StudentId = 5, CourseId = 1, Grade = 80, LetterGrade = "B", GradePoint = 3m, RowVersion = 0, Status = "A" },
                new Enrollment { Id = 4, StudentId = 3, CourseId = 1, Grade = 75, LetterGrade = "C", GradePoint = 2m, RowVersion = 0, Status = "A" },
                new Enrollment { Id = 5, StudentId = 4, CourseId = 1, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "A" }
            );
            _context.SaveChanges();
            _mockCourseRepository = new Mock<ICourseRepository>();

            // Repository 模擬回傳資料
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>())).Returns((int id) => _context.Courses.FirstOrDefault(s => s.Id == id));
            _mockCourseRepository.Setup(r => r.GetCourseByCode(It.IsAny<string>())).Returns((string code) => _context.Courses.FirstOrDefault(s => s.Code == code));

            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();

            _mockEnrollmentRepository.Setup(r => r.GetAllEnrollments()).Returns(_context.Enrollments.AsQueryable());

            _mockLogger = new Mock<ILogger<CourseService>>();
            _courseService = new CourseService(
                _context,
                _mockLogger.Object,
                _mockCourseRepository.Object,
                _mockEnrollmentRepository.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllCourses
        [Test] // 測試 GetAllCourses
        public void GetAllCourses_ReturnIsActiveStudents()
        {
            _mockCourseRepository.Setup(r => r.GetAllCourses()).Returns(_context.Courses.AsQueryable());
            var result = _courseService.GetAllCourses(1, 10);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Courses.Count, Is.EqualTo(3));
        }
        #endregion
        #region GetCourseById
        [Test] // 測試 GetCourseById => 存在
        public void GetCourseById_Existing_ReturnSuccess()
        {
            int id = 1;
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>()))
                .Returns((int id) => _context.Courses.FirstOrDefault(s => s.Id == id));
            var result = _courseService.GetCourseById(id);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Courses[0].Title, Is.EqualTo("B"));
        }
        [Test] // 測試 GetCourseById => Id 為 0
        public void GetCourseById_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>()))
                .Returns((int id) => _context.Courses.FirstOrDefault(s => s.Id == id));
            var result = _courseService.GetCourseById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Id為空"));
        }
        [Test] // 測試 GetCourseById => 不存在
        public void GetCourseById_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>()))
                .Returns((int id) => _context.Courses.FirstOrDefault(s => s.Id == id));
            var result = _courseService.GetCourseById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("無此Id課程"));
        }
        [Test] // 測試 GetCourseById => IsActive == false
        public void GetCourseById_NoIsActice_ReturnFail()
        {
            int id = 2;
            _mockCourseRepository.Setup(r => r.GetCourseById(It.IsAny<int>()))
                .Returns((int id) => _context.Courses.FirstOrDefault(s => s.Id == id));
            var result = _courseService.GetCourseById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("此Id課程已停用"));
        }
        #endregion

        #region AddCourse
        [Test] // 測試 AddCourse => 成功
        public void AddCourse_ReturnSuccess()
        {
            // 建立測試新增資料
            var addRequest = new CourseRequest { Code = "add", Title = "add", Credits = 3 };
            _mockCourseRepository.Setup(r => r.AddCourse(It.IsAny<Course>()))
                .Callback<Course>(c =>
                {
                    c.IsActive = true; // 確保 IsActive 設置
                    c.CreatedAt = DateTime.Now; // 確保 CreatedAt 設置
                    _context.Courses.Add(c);
                });

            var addResult = _courseService.AddCourse(addRequest);
            Assert.That(addResult.Success, Is.True);
            Assert.That(addResult.Message, Is.EqualTo("新增成功"));
            Assert.That(addResult.Courses.Count, Is.EqualTo(1));
        }
        [Test] // 測試 AddCourse => CourseRequest null
        public void AddCourse_NullRequest_ReturnFail()
        {
            // 建立測試新增資料（Null）
            CourseRequest addRequest = null;
            _mockCourseRepository.Setup(r => r.AddCourse(It.IsAny<Course>()))
                .Callback<Course>(s => _context.Courses.Add(s));

            var addResult = _courseService.AddCourse(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("新增Course資料為空"));
        }
        [Test] // 測試 AddCourse => Code 存在
        public void AddCourse_ExistingCode_ReturnFail()
        {
            // 建立測試新增資料
            var addRequest = new CourseRequest { Code = "A", Title = "add", Credits = 3 };
            _mockCourseRepository.Setup(r => r.AddCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Add(c));

            var addResult = _courseService.AddCourse(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("課程代碼已存在"));
        }
        [Test] // 測試 AddCourse => try catch
        public void AddCourse_DbSaveFailure_ReturnFail()
        {
            // 建立測試新增資料
            var addRequest = new CourseRequest { Code = "add", Title = "add", Credits = 3 };
            _mockCourseRepository.Setup(r => r.AddCourse(It.IsAny<Course>())).Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var addResult = _courseService.AddCourse(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("新增發生錯誤"));
        }
        #endregion

        #region UpdateCourse
        [Test] // 測試 UpdateCourse => 成功
        public void UpdateCourse_ReturnSuccess()
        {
            // 更新資料
            int id = 1;
            var updateRequest = new CourseRequest { Code = "", Title = "update", Credits = 0 };
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.True);
            Assert.That(updateResult.Message, Is.EqualTo("更新成功"));
            Assert.That(updateResult.Courses[0].Title, Is.EqualTo("update"));
        }
        [Test] // 測試 UpdateCourse => Id 為 0
        public void UpdateCourse_ZeroId_ReturnFail()
        {
            // 更新資料（Id == 0）
            int id = 0;
            var updateRequest = new CourseRequest { Code = "", Title = "update", Credits = 0 };
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id或更新項目為空"));
        }
        [Test] // 測試 UpdateCourse => CourseRequest Null
        public void UpdateCourse_NullRequest_ReturnFail()
        {
            // 更新資料（Request == Null）
            int id = 1;
            CourseRequest updateRequest = null;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id或更新項目為空"));
        }
        [Test] // 測試 UpdateCourse => Course 不存在
        public void UpdateCourse_NoExisting_ReturnFail()
        {
            // 更新資料
            int id = 99;
            var updateRequest = new CourseRequest { Code = "", Title = "update", Credits = 0 };
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(s => _context.Courses.Update(s));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Course資料為空"));
        }
        [Test] // 測試 UpdateCourse => Course 已停用
        public void UpdateCourse_NoIsActice_ReturnFail()
        {
            // 更新資料
            int id = 2;
            var updateRequest = new CourseRequest { Code = "", Title = "update", Credits = 0 };
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(s => _context.Courses.Update(s));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("此Id課程已停用"));
        }
        [Test] // 測試 UpdateCourse => try catch
        public void UpdateCourse_DbSaveFailure_ReturnFail()
        {
            // 更新資料
            int id = 1;
            var updateRequest = new CourseRequest { Code = "", Title = "update", Credits = 0 };
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var updateResult = _courseService.UpdateCourse(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("更新發生錯誤"));
        }
        #endregion

        #region DeleteCourse
        [Test] // 測試 DeleteCourse => 成功
        public void DeleteCourse_ReturnSuccess()
        {
            int id = 4;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除成功"));
            var course = _context.Courses.FirstOrDefault(s => s.Id == id);
            Assert.That(course.IsActive, Is.False);
        }
        [Test] // 測試 DeleteCourse => HasEnrollment && Enrollment.Status == "W " 成功
        public void DeleteCourse_HasEnrollmentIsWithDraw_ReturnSuccess()
        {
            int id = 3;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除成功"));
            var course = _context.Courses.FirstOrDefault(s => s.Id == id);
            Assert.That(course.IsActive, Is.False);
        }
        [Test] // 測試 DeleteCourse => Id 為 0
        public void DeleteCourse_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("Id為空"));
        }
        [Test] // 測試 DeleteCourse => Course 不存在
        public void DeleteCourse_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("無此Id課程"));
        }
        [Test] // 測試 DeleteCourse => HasEnrollment && Enrollment.Status == "A "
        public void DeleteCourse_HasEnrollmentNoWithDraw_ReturnFail()
        {
            int id = 1;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Callback<Course>(c => _context.Courses.Update(c));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("有被選課，無法刪除"));
        }
        [Test] // 測試 DeleteCourse => try catch
        public void DeleteCourse_DbSaveFailure_ReturnFail()
        {
            int id = 4;
            _mockCourseRepository.Setup(r => r.UpdateCourse(It.IsAny<Course>()))
                .Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var deleteResult = _courseService.DeleteCourse(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除發生錯誤"));
        }
        #endregion

    }
}
