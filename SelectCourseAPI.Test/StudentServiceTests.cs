using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;


namespace SelectCourseAPI.Test
{
    [TestFixture]
    public class StudentServiceTests
    {
        private Mock<IStudentRepository> _mockStudentRepository;
        private Mock<IEnrollmentRepository> _mockEnrollmentRepository;
        private Mock<ILogger<StudentService>> _mockLogger;
        private StudentService _studentService;
        private SelectCourseContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<SelectCourseContext>()
                .UseInMemoryDatabase(databaseName: "TestStudentDB").Options;
            _context = new SelectCourseContext(options);
            // 清掉舊資料，避免測試間互相干擾
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed 初始資料
            _context.Students.AddRange(
                new Student { Id = 1, FirstName = "A", LastName = "B", Email = "ab@example.com", IsActive = true },
                new Student { Id = 2, FirstName = "C", LastName = "D", Email = "cd@example.com", IsActive = false },
                new Student { Id = 3, FirstName = "E", LastName = "F", Email = "ef@example.com", IsActive = true },
                new Student { Id = 4, FirstName = "G", LastName = "H", Email = "gh@example.com", IsActive = true },
                new Student { Id = 5, FirstName = "I", LastName = "J", Email = "ij@example.com", IsActive = true }
            );
            _context.Enrollments.AddRange(
               new Enrollment { Id = 1, StudentId = 3, CourseId = 1, Grade = 90, LetterGrade = "A", GradePoint = 4m, RowVersion = 0, Status = "A" },
               new Enrollment { Id = 2, StudentId = 1, CourseId = 3, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "W" },
               new Enrollment { Id = 3, StudentId = 5, CourseId = 1, Grade = 80, LetterGrade = "B", GradePoint = 3m, RowVersion = 0, Status = "A" },
               new Enrollment { Id = 4, StudentId = 3, CourseId = 1, Grade = 75, LetterGrade = "C", GradePoint = 2m, RowVersion = 0, Status = "A" },
               new Enrollment { Id = 5, StudentId = 4, CourseId = 1, Grade = null, LetterGrade = null, GradePoint = null, RowVersion = 0, Status = "A" }
           );
            _context.SaveChanges();

            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();

            // Repository 模擬回傳資料
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            _mockStudentRepository.Setup(r => r.GetStudentByEmail(It.IsAny<string>()))
                .Returns((string email) => _context.Students.FirstOrDefault(s => s.Email == email));

            _mockEnrollmentRepository.Setup(r => r.GetAllEnrollments()).Returns(_context.Enrollments.AsQueryable());

            _mockLogger = new Mock<ILogger<StudentService>>();
            _studentService = new StudentService(
                _context,
                _mockLogger.Object,
                _mockStudentRepository.Object,
                _mockEnrollmentRepository.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetAllStudents
        [Test] // 測試 GetAllStudents
        public void GetAllStudents_ReturnIsActiveStudents()
        {
            _mockStudentRepository.Setup(r => r.GetAllStudents()).Returns(_context.Students.AsQueryable());
            var result = _studentService.GetAllStudents(1, 10);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Students.Count, Is.EqualTo(4));
        }
        #endregion
        #region GetStudentById
        [Test] // 測試 GetStudentsById => 存在
        public void GetStudentsById_Existing_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("查詢成功"));
            Assert.That(result.Students[0].FirstName, Is.EqualTo("A"));
        }
        [Test] // 測試 GetStudentsById => Id 為 0
        public void GetStudentsById_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Id為空"));
        }
        [Test] // 測試 GetStudentsById => 不存在
        public void GetStudentsById_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("無此Id學生"));
        }
        [Test] // 測試 GetStudentsById => IsActive == false
        public void GetStudentsById_NoIsActice_ReturnFail()
        {
            int id = 2;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("此Id學生已停用"));
        }
        #endregion

        #region AddStudent
        [Test] // 測試 AddStudent => 成功（Email不存在）
        public void AddStudent_ReturnSuccess()
        {
            // 建立測試新增資料
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s =>
                {
                    s.IsActive = true; // 確保 IsActive 設置
                    s.CreatedAt = DateTime.Now; // 確保 CreatedAt 設置
                    _context.Students.Add(s);
                });

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.True);
            Assert.That(addResult.Message, Is.EqualTo("新增成功"));
            Assert.That(addResult.Students.Count, Is.EqualTo(1));
        }
        [Test] // 測試 AddStudent => StudentRequest 為空
        public void AddStudent_NullRequest_ReturnFail()
        {
            // 建立測試新增資料（Null）
            StudentRequest addRequest = null;
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("新增Student資料為空"));
        }
        [Test] // 測試 AddStudent => FirstName null
        public void AddStudent_NullFirstName_ReturnFail()
        {
            // 建立測試新增資料（FirstName Null）
            var addRequest = new StudentRequest { FirstName = null, LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => LastName null
        public void AddStudent_NullLastName_ReturnFail()
        {
            // 建立測試新增資料（LastName Null）
            var addRequest = new StudentRequest { FirstName = "add", LastName = null, Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => Email null
        public void AddStudent_NullEmail_ReturnFail()
        {
            // 建立測試新增資料（Email Null）
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = null };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => FirstName empty
        public void AddStudent_EmptyFirstName_ReturnFail()
        {
            // 建立測試新增資料（FirstName Null）
            var addRequest = new StudentRequest { FirstName = "" ,LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => LastName empty
        public void AddStudent_EmptyLastName_ReturnFail()
        {
            // 建立測試新增資料（LastName Null）
            var addRequest = new StudentRequest { FirstName = "add", LastName = "", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => Email empty
        public void AddStudent_EmptyEmail_ReturnFail()
        {
            // 建立測試新增資料（Email Null）
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("必填欄位不能為空"));
        }
        [Test] // 測試 AddStudent => Email 存在
        public void AddStudent_ExistingEmail_ReturnFail()
        {
            // 建立測試新增資料
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "ab@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("Email已存在"));
        }
        [Test] // 測試 AddStudent => Email 格式不正確
        public void AddStudent_IncorrectFormatEmail_ReturnFail()
        {
            // 建立測試新增資料（Email 格式不正確）
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("Email格式不正確"));
        }
        [Test] // 測試 AddStudent => try catch
        public void AddStudent_DbSaveFailure_ReturnFail()
        {
            // 建立測試新增資料
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>())).Throws(new DbUpdateException("模擬資料庫儲存失敗"));
            
            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("新增發生錯誤"));
        }
        #endregion

        #region UpdateStudent
        [Test] // 測試 UpdateStudent => 成功
        public void UpdateStudent_ReturnSuccess()
        {
            // 更新資料
            int id = 1;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.True);
            Assert.That(updateResult.Message, Is.EqualTo("更新成功"));
            Assert.That(updateResult.Students[0].LastName, Is.EqualTo("update"));
        }
        [Test] // 測試 UpdateStudent => Id 為 0
        public void UpdateStudent_ZeroId_ReturnFail()
        {
            // 更新資料（Id == 0）
            int id = 0;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id或更新項目為空"));
        }
        [Test] // 測試 UpdateStudent => StudentRequest Null
        public void UpdateStudent_NullRequest_ReturnFail()
        {
            // 更新資料（Request == Null）
            int id = 1;
            StudentRequest updateRequest = null;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id或更新項目為空"));
        }
        [Test] // 測試 UpdateStudent => Student 不存在
        public void UpdateStudent_NoExisting_ReturnFail()
        {
            // 更新資料
            int id = 99;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("此Id的Student資料為空"));
        }
        [Test] // 測試 UpdateStudent => Student 已停用
        public void UpdateStudent_NoIsActice_ReturnFail()
        {
            // 更新資料
            int id = 2;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("此Id學生已停用"));
        }
        [Test] // 測試 UpdateStudent => try catch
        public void UpdateStudent_DbSaveFailure_ReturnFail()
        {
            // 更新資料
            int id = 1;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("更新發生錯誤"));
        }
        #endregion

        #region DeleteStudent
        [Test] // 測試 DeleteStudent => 成功
        public void DeleteStudent_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除成功"));
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            Assert.That(student.IsActive, Is.False);
        }
        [Test] // 測試 DeleteStudent => HasEnrollment && Enrollment.Status == "W " 成功
        public void DeleteStudent_HasEnrollmentIsWithDraw_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除成功"));
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            Assert.That(student.IsActive, Is.False);
        }
        [Test] // 測試 DeleteStudent => Id 為 0
        public void DeleteStudent_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("Id為空"));
        }
        [Test] // 測試 DeleteStudent => Student 不存在
        public void DeleteStudent_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("無此Id學生"));
        }
        [Test] // 測試 DeleteStudent => HasEnrollment && Enrollment.Status == "A "
        public void DeleteStudent_HasEnrollmentNoWithDraw_ReturnFail()
        {
            int id = 3;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("還有課程，無法刪除"));
        }
        [Test] // 測試 DeleteStudent => try catch
        public void DeleteStudent_DbSaveFailure_ReturnFail()
        {
            int id = 2;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Throws(new DbUpdateException("模擬資料庫儲存失敗"));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("刪除發生錯誤"));
        }
        #endregion
    }
}