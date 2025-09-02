namespace SelectCourseAPI.Dto.Response
{
    public class StudentResponse : BaseResponse
    {
        public List<StudentDto>? Students { get; set; }
    }
    public class StudentDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        //public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
        //public bool? IsActive { get; set; }
    }
}
