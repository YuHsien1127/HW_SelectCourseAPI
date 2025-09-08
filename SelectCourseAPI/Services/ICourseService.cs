using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;

namespace SelectCourseAPI.Services
{
    public interface ICourseService
    {
        public CourseResponse GetAllCourses(int page, int pageSize);
        public CourseResponse GetCourseById(int id);
        public CourseResponse AddCourse(CourseRequest courseRequest);
        public CourseResponse UpdateCourse(int id, CourseRequest courseRequest);
        public CourseResponse StopCourse(int id);
        public CourseResponse DeleteCourse(int id);
    }
}
