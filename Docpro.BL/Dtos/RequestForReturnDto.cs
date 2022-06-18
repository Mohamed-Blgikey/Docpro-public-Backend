using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class RequestForReturnDto
    {
        public int id { get; set; }
        public DateTime Created { get; set; }
        public string Degree { get; set; }
        public string Descraption { get; set; }
        public string PatientId { get; set; }
        public UserForReturnDto Patient { get; set; }
    }
}
