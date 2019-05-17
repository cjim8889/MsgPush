using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelePush.Api.Service
{
    public class HookValidationResponseDAO
    {
        public string Message { get; set; }
    }
    public class HookService
    {
        private HttpClient httpClient;

        public HookService()
        {
            this.httpClient = new HttpClient();
        }

        public async Task<bool> ValidateHook(string hook)
        {
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("message", "ping"),
            });

            var response = await httpClient.PostAsync(hook, formData);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var responseStr = await response.Content.ReadAsStringAsync();
            try
            {
                var responseObj = JsonConvert.DeserializeObject<HookValidationResponseDAO>(responseStr);
                if (string.IsNullOrWhiteSpace(responseObj.Message))
                {
                    return false;
                }

                return responseObj.Message == "pong";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
