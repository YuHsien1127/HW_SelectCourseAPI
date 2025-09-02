using System;
using System.Collections.Generic;

namespace SelectCourseAPI.Models
{
    public partial class Enrollment
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? Grade { get; set; }
        public string? LetterGrade { get; set; }
        public decimal? GradePoint { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public virtual Course Course { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
