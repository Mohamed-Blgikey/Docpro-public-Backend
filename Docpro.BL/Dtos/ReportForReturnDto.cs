using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class ReportForReturnDto
    {
        public int Id { get; set; }
        public string Day { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Diagnosis { get; set; }
        public string treatment { get; set; }

        public DateTime Date { get; set; }
        public string DoctorId { get; set; }
        public UserForReturnDto Doctor { get; set; }

        public string PatientId { get; set; }
        public UserForReturnDto Patient { get; set; }
    }
}
