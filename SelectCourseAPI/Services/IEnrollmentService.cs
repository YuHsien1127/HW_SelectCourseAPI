using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;

namespace SelectCourseAPI.Services
{
    public interface IEnrollmentService
    {
        public EnrollmentResponse GetAllEnrollments(int page, int pageSize);
        public EnrollmentResponse GetEnrollmentById(int studentId, int courseId);
        public EnrollmentResponse Enroll(int studentId, int courseId);
        public EnrollmentResponse UpdateGrade(EnrollmentRequest enrollmentRequest);
        public EnrollmentResponse Withdraw(int studentId, int courseId);
    }
}
