using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public string Loc { get; set; } = null!;
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
        public ushort SemesterYear { get; set; }
        public string SemesterSeason { get; set; } = null!;
        public string Teacher { get; set; } = null!;
        public uint CId { get; set; }
        public uint ClassId { get; set; }

        public virtual Course CIdNavigation { get; set; } = null!;
        public virtual Professor TeacherNavigation { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
