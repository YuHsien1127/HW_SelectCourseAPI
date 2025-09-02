using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SelectCourseContext _context;
        public StudentRepository(SelectCourseContext context)
        {
            _context = context;
        }

        public IQueryable<Student> GetAllStudents()
        {
            return _context.Students;
        }

        public Student GetStudentById(int id)
        {
            return _context.Students.Find(id);
        }

        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
        }

        public void DeleteStudent(Student student)
        {
            _context.Students.Remove(student);
        }
       
        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }
    }
}
