using System;
using System.Collections.Generic;
using System.Text;

namespace TelePush.Backend.Model
{
    class MqMessage
    {
        public string Content { get; set; }
        public List<long> Receivers { get; set; }
        public string Hook { get; set; }
    }
}
