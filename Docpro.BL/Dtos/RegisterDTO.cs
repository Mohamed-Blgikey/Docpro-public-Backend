using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class RegisterDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        [MinLength(6)]
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public DateTime BirthDate { get; set; }
        //[MinLength(11),MaxLength(11),RegularExpression("^01[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$")]
        //public string PhoneNumber { get; set; }
        //public bool Gender { get; set; }
    }
}
