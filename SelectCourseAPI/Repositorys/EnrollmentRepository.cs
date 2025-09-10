using Microsoft.EntityFrameworkCore;
using SelectCourseAPI.Models;

namespace SelectCourseAPI.Repositorys
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly SelectCourseContext _context;
        public EnrollmentRepository(SelectCourseContext context)
        {
            _context = context;
        }

        public IQueryable<Enrollment> GetAllEnrollments()
        {
            return _context.Enrollments.Include(s => s.Student).Include(c => c.Course);
        }
        public Enrollment GetEnrollmentById(int studentId, int courseId)
        {
            return _context.Enrollments.Include(s => s.Student).Include(c => c.Course)
                .FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId);
        }
        public Enrollment GetEnrollmentByStudentId(int studentId)
        {
            return _context.Enrollments.Include(s => s.Student).Include(c => c.Course)
                .FirstOrDefault(x => x.StudentId == studentId);
        }

        public void AddEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
        }

        public void DeleteEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Remove(enrollment);
        }
        
        public void UpdateEnrollment(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
        }
    }
}
