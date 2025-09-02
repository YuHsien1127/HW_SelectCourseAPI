using SelectCourseAPI.Dto.Response;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;

namespace SelectCourseAPI.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly SelectCourseContext _context;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IEnrollmentRepository enrollmentRepository;
        public EnrollmentService(SelectCourseContext context, ILogger<EnrollmentService> logger, IEnrollmentRepository enrollmentRepository)
        {
            _context = context;
            _logger = logger;
            this.enrollmentRepository = enrollmentRepository;
        }

        public IQueryable<Enrollment> GetAllEnrollments()
        {

        }
        public Enrollment GetEnrollmentById(int studentId, int courseId)
        {

        }
        public EnrollmentResponse Enroll(int studentId, int courseId)
        {

        }
        public EnrollmentResponse UpdateGrade(int studentId, int courseId, int grade, byte[] rowVersion)
        {

        }
        public EnrollmentResponse Withdraw(int studentId, int courseId)
        {

        }
    }
}
