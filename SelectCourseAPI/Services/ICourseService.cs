﻿using SelectCourseAPI.Dto.Request;
using SelectCourseAPI.Dto.Response;

namespace SelectCourseAPI.Services
{
    public interface ICourseService
    {
        public CourseResponse GetAllCourses();
        public CourseResponse GetCourseById(int id);
        public CourseResponse AddCourse(CourseRequest courseRequest);
        public CourseResponse UpdateCourse(int id, CourseRequest courseRequest);
        public CourseResponse DeleteCourse(int id);
    }
}
