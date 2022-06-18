using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class AddDoctorToSectionDto
    {
        [EmailAddress]
        public string Email { get; set; }
        public int sectionId { get; set; }
    }
}
