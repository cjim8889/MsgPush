using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MsgPush.Service
{

    public interface IAuthService
    {
        bool ContainsKey(string key);
        bool Authenticate(string key, string challengeCode);
        void RemoveKey(string key);
        void New(string key);
    }

    public class AuthService : IAuthService
    {
        private readonly Dictionary<string, string> vault;

        public AuthService()
        {
            vault = new Dictionary<string, string>();
        }
        public bool Authenticate(string key, string challengeCode)
        {
            if (ContainsKey(key))
            {
                if (vault.GetValueOrDefault(key) == challengeCode)
                {
                    RemoveKey(key);
                    return true;
                }
            }

            return false;
        }

        public bool ContainsKey(string key)
        {
            return vault.ContainsKey(key);
        }

        public void RemoveKey(string key)
        {
            vault.Remove(key);
        }

        public void New(string key)
        {
            var code = GenerateChallengeCode();
            Console.WriteLine(code);
            
            vault.Add(key, code);
        }

        private string GenerateChallengeCode()
        {
            var random = new Random();

            return random.Next(100000, 1000000).ToString();
        }
    }
}
