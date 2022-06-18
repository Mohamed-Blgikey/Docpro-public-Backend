using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docpro.BL.Helper
{
    public class PagedParams
    {
        private const int maxSize = 50;
        public int PageNumber { get; set; } = 1;

        private int pageSize = 20;

        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value > 50 ? maxSize :  value; }
        }


    }
}
