using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public interface ICourseRepository
    {
        public IQueryable<Course> GetAllCourses();
        public Course GetCourseById(int id);
        public Course GetCourseByCode(string code);
        public void AddCourse(Course course);
        public void UpdateCourse(Course course);
        public void DeleteCourse(Course course);
    }
}
