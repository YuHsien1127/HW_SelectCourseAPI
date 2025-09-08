using System;
using System.Collections.Generic;

namespace SelectCourseAPI.Models
{
    public partial class Course
    {
        public Course()
        {
            Enrollments = new HashSet<Enrollment>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int Credits { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}
