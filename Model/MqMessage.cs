using System.Collections.Generic;

namespace MsgPush.Model
{
    public class MqMessage
    {
        public string Content { get; set; }
        public List<long> Receivers { get; set; }
    }
}
