namespace SelectCourseAPI.Dto.Request
{
    public class CourseRequest
    {
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Credits { get; set; }
    }
}
