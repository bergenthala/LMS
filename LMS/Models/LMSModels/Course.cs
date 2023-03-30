using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public string Name { get; set; } = null!;
        public uint CId { get; set; }
        public uint Number { get; set; }
        public string DeptId { get; set; } = null!;

        public virtual Department Dept { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
