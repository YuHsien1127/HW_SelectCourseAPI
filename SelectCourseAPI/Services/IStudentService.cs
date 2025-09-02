using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;

namespace SelectCourseAPI.Services
{
    public interface IStudentService
    {
        public StudentResponse GetAllStudents();
        public StudentResponse GetStudentById(int id);
        public StudentResponse AddStudent(StudentRequest studentRequest);
        public StudentResponse UpdateStudent(int id, StudentRequest studentRequest);
        public StudentResponse DeleteStudent(int id);
    }
}
