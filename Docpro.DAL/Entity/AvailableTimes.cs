using Docpro.DAL.extend;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Entity
{
    public class AvailableTimes
    {
        public int Id { get; set; }
        [RegularExpression("^[a-zA-z]{3,}$")]
        public string Day { get; set; }
        [RegularExpression("^(1|2|3|4|5|6|8|7|9|10|11|12)(pm|am|PM|AM)$")]
        public string From { get; set; }
        [RegularExpression("^(1|2|3|4|5|6|8|7|9|10|11|12)(pm|am|PM|AM)$")]
        public string To { get; set; }
        public string doctorId { get; set; }
        public ApplicationUser Doctor { get; set; }
    }
}
