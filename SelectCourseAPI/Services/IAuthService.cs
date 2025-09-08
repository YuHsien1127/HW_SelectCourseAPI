namespace SelectCourseAPI.Services
{
    public interface IAuthService
    {
        public string GenerateJwtToken(string email, string role);
    }
}
