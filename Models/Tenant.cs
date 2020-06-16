using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Tenant
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public Address Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string EMail { get; set; }
        public string WWW { get; set; }

        public Tenant GlobalTenant { get; set; }
    }
}
