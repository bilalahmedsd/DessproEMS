using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS.Core.Models.ForFaith
{
    public class FourFaithMqttPayloadDTO
    {
        public string did { get; set; }
        public string utime { get; set; }
        public List<FourFaithMqttContentDTO> content { get; set; } = new();
    }
}
