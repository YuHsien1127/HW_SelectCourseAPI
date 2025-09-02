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
            return _context.Enrollments;
        }

        public Enrollment GetEnrollmentById(int studentId, int courseId)
        {
            return _context.Enrollments.Include(s => s.StudentId).Include(c => c.CourseId)
                .FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId);
        }

        public void AddStudent(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
        }

        public void DeleteStudent(Enrollment enrollment)
        {
            _context.Enrollments.Remove(enrollment);
        }
        
        public void UpdateStudent(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
        }
    }
}
