
namespace EMS.CronJobs.ForFaith
{
    public class FourFaithMqttPayloadDTO
    {
        public string did { get; set; }
        public string utime { get; set; }
        public List<FourFaithMqttContentDTO> content { get; set; } = new();
    }
}
