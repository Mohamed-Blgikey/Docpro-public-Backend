using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class SectionForReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string PhotoName { set; get; }

        public IEnumerable<UserForReturnDto> Doctors { set; get; }
    }
}
