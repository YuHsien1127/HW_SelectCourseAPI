using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public interface IEnrollmentRepository
    {
        public IQueryable<Enrollment> GetAllEnrollments();
        public Enrollment GetEnrollmentById(int studentId, int courseId);
        public void AddStudent(Enrollment enrollment);
        public void UpdateStudent(Enrollment enrollment);
        public void DeleteStudent(Enrollment enrollment);
    }
}
