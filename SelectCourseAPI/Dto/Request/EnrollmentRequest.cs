namespace SelectCourseAPI.Dto.Request
{
    public class EnrollmentRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? Grade { get; set; }
    }
}
