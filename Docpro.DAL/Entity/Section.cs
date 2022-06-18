using Docpro.DAL.extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Entity
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string PhotoName { set; get; }

        //public string Title { set; get; }
        //public string DoctorsId { set; get; }
        public IEnumerable<ApplicationUser> Doctors { set; get; }
    }
}
