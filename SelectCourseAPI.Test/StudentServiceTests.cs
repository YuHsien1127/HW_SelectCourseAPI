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
            // �M���¸�ơA�קK���ն����ۤz�Z
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed ��l���
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

            // Repository �����^�Ǹ��
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>())).Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            _mockStudentRepository.Setup(r => r.GetStudentByEmail(It.IsAny<string>())).Returns((string email) => _context.Students.FirstOrDefault(s => s.Email == email));

            _mockEnrollmentRepository = new Mock<IEnrollmentRepository>();

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
        [Test] // ���� GetAllStudents
        public void GetAllStudents_ReturnIsActiveStudents()
        {
            _mockStudentRepository.Setup(r => r.GetAllStudents()).Returns(_context.Students.AsQueryable());
            var result = _studentService.GetAllStudents(1, 10);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("�d�ߦ��\"));
            Assert.That(result.Students.Count, Is.EqualTo(4));
        }
        #endregion
        #region GetStudentById
        [Test] // ���� GetStudentsById => �s�b
        public void GetStudentsById_Existing_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.EqualTo("�d�ߦ��\"));
            Assert.That(result.Students[0].FirstName, Is.EqualTo("A"));
        }
        [Test] // ���� GetStudentsById => Id �� 0
        public void GetStudentsById_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("Id����"));
        }
        [Test] // ���� GetStudentsById => ���s�b
        public void GetStudentsById_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("�L��Id�ǥ�"));
        }
        [Test] // ���� GetStudentsById => IsActive == false
        public void GetStudentsById_NoIsActice_ReturnFail()
        {
            int id = 2;
            _mockStudentRepository.Setup(r => r.GetStudentById(It.IsAny<int>()))
                .Returns((int id) => _context.Students.FirstOrDefault(s => s.Id == id));
            var result = _studentService.GetStudentById(id);
            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("��Id�ǥͤw����"));
        }
        #endregion

        #region AddStudent
        [Test] // ���� AddStudent => ���\�]Email���s�b�^
        public void AddStudent_ReturnSuccess()
        {
            // �إߴ��շs�W���
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s =>
                {
                    s.IsActive = true; // �T�O IsActive �]�m
                    s.CreatedAt = DateTime.Now; // �T�O CreatedAt �]�m
                    _context.Students.Add(s);
                });

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.True);
            Assert.That(addResult.Message, Is.EqualTo("�s�W���\"));
            Assert.That(addResult.Students.Count, Is.EqualTo(1));
        }
        [Test] // ���� AddStudent => StudentRequest ����
        public void AddStudent_NullRequest_ReturnFail()
        {
            // �إߴ��շs�W��ơ]Null�^
            StudentRequest addRequest = null;
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("�s�WStudent��Ƭ���"));
        }
        [Test] // ���� AddStudent => FirstName null
        public void AddStudent_NullFirstName_ReturnFail()
        {
            // �إߴ��շs�W��ơ]FirstName Null�^
            var addRequest = new StudentRequest { FirstName = null, LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => LastName null
        public void AddStudent_NullLastName_ReturnFail()
        {
            // �إߴ��շs�W��ơ]LastName Null�^
            var addRequest = new StudentRequest { FirstName = "add", LastName = null, Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => Email null
        public void AddStudent_NullEmail_ReturnFail()
        {
            // �إߴ��շs�W��ơ]Email Null�^
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = null };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => FirstName empty
        public void AddStudent_EmptyFirstName_ReturnFail()
        {
            // �إߴ��շs�W��ơ]FirstName Null�^
            var addRequest = new StudentRequest { FirstName = "" ,LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => LastName empty
        public void AddStudent_EmptyLastName_ReturnFail()
        {
            // �إߴ��շs�W��ơ]LastName Null�^
            var addRequest = new StudentRequest { FirstName = "add", LastName = "", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => Email empty
        public void AddStudent_EmptyEmail_ReturnFail()
        {
            // �إߴ��շs�W��ơ]Email Null�^
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("������줣�ର��"));
        }
        [Test] // ���� AddStudent => Email �s�b
        public void AddStudent_ExistingEmail_ReturnFail()
        {
            // �إߴ��շs�W���
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "ab@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("Email�w�s�b"));
        }
        [Test] // ���� AddStudent => Email �榡�����T
        public void AddStudent_IncorrectFormatEmail_ReturnFail()
        {
            // �إߴ��շs�W��ơ]Email �榡�����T�^
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Add(s));

            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("Email�榡�����T"));
        }
        [Test] // ���� AddStudent => try catch
        public void AddStudent_DbSaveFailure_ReturnFail()
        {
            // �إߴ��շs�W���
            var addRequest = new StudentRequest { FirstName = "add", LastName = "add", Email = "add@example.com" };
            _mockStudentRepository.Setup(r => r.AddStudent(It.IsAny<Student>())).Throws(new DbUpdateException("������Ʈw�x�s����"));
            
            var addResult = _studentService.AddStudent(addRequest);
            Assert.That(addResult.Success, Is.False);
            Assert.That(addResult.Message, Is.EqualTo("�s�W�o�Ϳ��~"));
        }
        #endregion

        #region UpdateStudent
        [Test] // ���� UpdateStudent => ���\
        public void UpdateStudent_ReturnSuccess()
        {
            // ��s���
            int id = 1;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.True);
            Assert.That(updateResult.Message, Is.EqualTo("��s���\"));
            Assert.That(updateResult.Students[0].LastName, Is.EqualTo("update"));
        }
        [Test] // ���� UpdateStudent => Id �� 0
        public void UpdateStudent_ZeroId_ReturnFail()
        {
            // ��s��ơ]Id == 0�^
            int id = 0;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id�Χ�s���ج���"));
        }
        [Test] // ���� UpdateStudent => StudentRequest Null
        public void UpdateStudent_NullRequest_ReturnFail()
        {
            // ��s��ơ]Request == Null�^
            int id = 1;
            StudentRequest updateRequest = null;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("Id�Χ�s���ج���"));
        }
        [Test] // ���� UpdateStudent => Student ���s�b
        public void UpdateStudent_NoExisting_ReturnFail()
        {
            // ��s���
            int id = 99;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("��Id��Student��Ƭ���"));
        }
        [Test] // ���� UpdateStudent => Student �w����
        public void UpdateStudent_NoIsActice_ReturnFail()
        {
            // ��s���
            int id = 2;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("��Id�ǥͤw����"));
        }
        [Test] // ���� UpdateStudent => try catch
        public void UpdateStudent_DbSaveFailure_ReturnFail()
        {
            // ��s���
            int id = 1;
            var updateRequest = new StudentRequest { FirstName = "", LastName = "update", Email = "" };
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Throws(new DbUpdateException("������Ʈw�x�s����"));

            var updateResult = _studentService.UpdateStudent(id, updateRequest);
            Assert.That(updateResult.Success, Is.False);
            Assert.That(updateResult.Message, Is.EqualTo("��s�o�Ϳ��~"));
        }
        #endregion

        #region DeleteStudent
        [Test] // ���� DeleteStudent => ���\
        public void DeleteStudent_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("�R�����\"));
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            Assert.That(student.IsActive, Is.False);
        }
        [Test] // ���� DeleteStudent => HasEnrollment && Enrollment.Status == "W " ���\
        public void DeleteStudent_HasEnrollmentIsWithDraw_ReturnSuccess()
        {
            int id = 1;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.True);
            Assert.That(deleteResult.Message, Is.EqualTo("�R�����\"));
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            Assert.That(student.IsActive, Is.False);
        }
        [Test] // ���� DeleteStudent => Id �� 0
        public void DeleteStudent_ZeroId_ReturnFail()
        {
            int id = 0;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("Id����"));
        }
        [Test] // ���� DeleteStudent => Student ���s�b
        public void DeleteStudent_NoExisting_ReturnFail()
        {
            int id = 99;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("�L��Id�ǥ�"));
        }
        [Test] // ���� DeleteStudent => HasEnrollment && Enrollment.Status == "A "
        public void DeleteStudent_HasEnrollmentNoWithDraw_ReturnFail()
        {
            int id = 3;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Callback<Student>(s => _context.Students.Update(s));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("�٦��ҵ{�A�L�k�R��"));
        }
        [Test] // ���� DeleteStudent => try catch
        public void DeleteStudent_DbSaveFailure_ReturnFail()
        {
            int id = 2;
            _mockStudentRepository.Setup(r => r.UpdateStudent(It.IsAny<Student>()))
                .Throws(new DbUpdateException("������Ʈw�x�s����"));

            var deleteResult = _studentService.DeleteStudent(id);
            Assert.That(deleteResult.Success, Is.False);
            Assert.That(deleteResult.Message, Is.EqualTo("�R���o�Ϳ��~"));
        }
        #endregion
    }
}