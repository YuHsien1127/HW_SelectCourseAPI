namespace SelectCourseAPI.Services
{
    public interface IExcelService
    {
        public byte[] ExcelEnroll_CourseByStudentId(int studentId);
        public byte[] ExcelEnroll_StudentByCourseId(int courseId);
    }
}
