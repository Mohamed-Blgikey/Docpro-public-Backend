using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class ReservationForReturnDto
    {
        public string Day { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public bool Index { get; set; }
        public DateTime Date { get; set; }
        public string DoctorId { get; set; }
        public UserForReturnDto Doctor { get; set; }

        public string PatientId { get; set; }
        public UserForReturnDto Patient { get; set; }
    }
}
