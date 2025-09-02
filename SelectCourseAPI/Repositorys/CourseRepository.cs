using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public class CourseRepository : ICourseRepository
    {
        private readonly SelectCourseContext _context;
        public CourseRepository(SelectCourseContext context)
        {
            _context = context;
        }

        public IQueryable<Course> GetAllCourses()
        {
            return _context.Courses;
        }

        public Course GetCourseById(int id)
        {
            return _context.Courses.Find(id);
        }

        public Course GetCourseByCode(string code)
        {
            return _context.Courses.FirstOrDefault(c => c.Code == code);
        }
        public void AddCourse(Course course)
        {
            _context.Courses.Add(course);
        }

        public void DeleteCourse(Course course)
        {
            _context.Courses.Remove(course);
        }
        
        public void UpdateCourse(Course course)
        {
            _context.Courses.Update(course);
        }
    }
}
