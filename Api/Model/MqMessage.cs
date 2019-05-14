using System.Collections.Generic;

namespace TelePush.Api.Model
{
    public class MqMessage
    {
        public string Content { get; set; }
        public List<long> Receivers { get; set; }
    }
}
