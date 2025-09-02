namespace SelectCourseAPI.Dto.Response
{
    public class EnrollmentResponse : BaseResponse
    {
        public List<EnrollmentDto>? Enrollments { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }
    }
    public class EnrollmentDto
    {
        public int StudentId { get; set; }
        public StudentDto? Students { get; set; }
        public int CourseId { get; set; }
        public CourseDto? Courses { get; set; }
        public int? Grade { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
        //public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
        //public int RowVersion { get; set; };
    }
}
