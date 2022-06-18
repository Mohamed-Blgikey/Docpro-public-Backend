using Docpro.DAL.extend;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Entity
{
    public class Book
    {
        public Book()
        {
            Date = DateTime.Now;
        }
        public string Day { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Index { get; set; }
        [MaxLength(1)]
        public string? Status { get; set; }

        public DateTime Date { get; set; }
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }

        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
    }

}
