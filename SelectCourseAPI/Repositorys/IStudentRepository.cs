using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public interface IStudentRepository
    {
        public IQueryable<Student> GetAllStudents();
        public Student GetStudentById(int id);
        public void AddStudent(Student student);
        public void UpdateStudent(Student student);
        public void DeleteStudent(Student student);

    }
}
