using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class UserForReturnDto
    {
        public string Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }

        public string FullName { get; set; }
        public string PhotoName { get; set; }
        public string Status { get; set; }
        public string Degree { get; set; }
        public int sectionId { get; set; }
    }
}
