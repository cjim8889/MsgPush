using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Backend.Service
{
    class HookService
    {
        private Dictionary<int, string> hooksTable;
        private HttpClient httpClient;

        public HookService()
        {
            hooksTable = new Dictionary<int, string>();
            this.httpClient = new HttpClient();
        }

        public void AddHook(int messageId, string hook)
        {
            hooksTable.Add(messageId, hook);
        }

        public string GetHook(int messageId)
        {
            return hooksTable.GetValueOrDefault(messageId);
        }

        public async void InvokeHook(int messageId, string message)
        {
            //var hook = GetHook(messageId);
            var hook = "http://localhost:5000/test";
            if (hook == null)
            {
                return;
            }

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("message", message),
            });


            var response = await httpClient.PostAsync(hook, formData);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}
