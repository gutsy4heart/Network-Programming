using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class RequestModel
    {
        public string Username { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Username} : {Message}";
        }
    }
}
