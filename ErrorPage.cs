using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrokenLinkChecker
{
    internal class ErrorPage
    {
        public string PageURL { get; set; }
        public string ResourceURL { get; set; }
        public int ErrorCode { get; set; }
    }
}
