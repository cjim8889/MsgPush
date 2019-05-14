using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TelePush.Api.Model
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
        InvalidUsernameOrPassword = 3,
        DuplicateUsername = 4,
        InvalidRecaptchaToken = 5,
        EmptyRecaptchaToken = 6,
        EmptyUsernameOrPassword = 7
    }

    public static class ResponseMessage
    {
        public static string InvalidAdminToken = "Invalid Admin Token"; 
        public static string EmptyUsernameOrPassword = "Empty Password or Username";
        public static string InvalidUsernameOrPassword = "Invalid Username Or Password";
        public static string DuplicateUsername = "Duplicate Username";

        public static string InvalidRecaptchaToken = "Invalid Captcha Token";
        public static string EmptyRecaptchaToken = "Empty Captcha Token";
    }
}
