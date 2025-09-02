namespace SelectCourseAPI.Dto.Response
{
    public class CourseResponse : BaseResponse
    {
        public List<CourseDto>? Courses { get; set; }
    }
    public class CourseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Credits { get; set; }
        //public bool? IsActive { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }
}
