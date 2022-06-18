using Docpro.DAL.extend;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Entity
{
    public class Request
    {
        public Request()
        {
            Created = DateTime.Now; 
        }

        public int id { get; set; }
        public DateTime Created { get; set; }
        public string Degree { get; set; }
        public string Descraption { get; set; }
        public string PatientId { get; set; }
        public ApplicationUser Patient { get; set; }
    }
}
