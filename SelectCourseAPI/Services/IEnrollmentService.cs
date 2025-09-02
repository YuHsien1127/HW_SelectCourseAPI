using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;

namespace SelectCourseAPI.Services
{
    public interface IEnrollmentService
    {
        public IQueryable<Enrollment> GetAllEnrollments();
        public Enrollment GetEnrollmentById(int studentId, int courseId);
        public EnrollmentResponse Enroll(int studentId, int courseId);
        public EnrollmentResponse UpdateGrade(int studentId, int courseId, int grade, byte[] rowVersion);
        public EnrollmentResponse Withdraw(int studentId, int courseId);
    }
}
