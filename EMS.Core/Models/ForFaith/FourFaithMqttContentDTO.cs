using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models.ForFaith
{
    public class FourFaithMqttContentDTO
    {

        public string pid { get; set; }
        public string type { get; set; }
        public string addr { get; set; }
        public float addrv { get; set; }
        public string ctime { get; set; }
    }
}
