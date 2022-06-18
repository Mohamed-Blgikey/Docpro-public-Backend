using Docpro.DAL.extend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.DAL.Entity
{
    public class Post
    {
        public Post()
        {
            Date = DateTime.Now;
        }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Topic { get; set; }
        public string Catpion { get; set; }
        public string PhotoName { get; set; }
        public string DoctorId { get; set; }
        public ApplicationUser Doctor { get; set; }
    }
}
