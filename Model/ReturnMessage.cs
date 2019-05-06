using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsgPush.Model
{
    public class ReturnMessage
    {
        public StatusCode StatusCode { get; set; }
        public object Message { get; set; }
    }

    public enum StatusCode
    {
        InvalidAdminToken = 1,
        Success = 2,
        InvalidEmailOrPassword = 3,
        DuplicateEmail = 4,
        InvalidRecaptchaToken = 5,
        EmptyRecaptchaToken = 6
    }
}
