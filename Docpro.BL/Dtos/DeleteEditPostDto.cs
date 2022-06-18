using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Dtos
{
    public class DeleteEditPostDto
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Catpion { get; set; }
        public string PhotoName { get; set; }
        public string DoctorId { get; set; }
    }
}
