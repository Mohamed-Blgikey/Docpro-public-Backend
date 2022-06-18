using Docpro.DAL.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.extend
{
    public class ApplicationUser:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string PhotoName { get; set; }
        public string Status { get; set; }
        public string? Degree { get; set; }

        public int? SectionId { get; set; }
        public Section Section { get; set; }

        public Request Request { get; set; }

        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<AvailableTimes> AvailableTimes { get; set; }
        public IEnumerable<Book> DoctorBooks { get; set; }
        public IEnumerable<Book> PatientBooks { get; set; }

        public IEnumerable<BooKRepot> DoctorReports { get; set; }
        public IEnumerable<BooKRepot> PatientReports { get; set; }

    }
}
