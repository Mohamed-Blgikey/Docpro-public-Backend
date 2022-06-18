using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class CreateRequestDto
    {
        public string Degree { get; set; }
        public string Descraption { get; set; }
        public string PatientId { get; set; }
    }
}
