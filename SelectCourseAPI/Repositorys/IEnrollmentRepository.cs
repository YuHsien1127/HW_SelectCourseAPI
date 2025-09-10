using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public interface IEnrollmentRepository
    {
        public IQueryable<Enrollment> GetAllEnrollments();
        public Enrollment GetEnrollmentById(int studentId, int courseId);
        public Enrollment GetEnrollmentByStudentId(int studentId);
        public void AddEnrollment(Enrollment enrollment);
        public void UpdateEnrollment(Enrollment enrollment);
        public void DeleteEnrollment(Enrollment enrollment);
    }
}
