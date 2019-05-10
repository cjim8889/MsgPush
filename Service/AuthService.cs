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
        void New(string key, long subsriberId);

        long GetSubsriberId(string key);
    }

    public class AuthService : IAuthService
    {
        private readonly Dictionary<string, (string, long)> vault;

        public AuthService()
        {
            vault = new Dictionary<string, (string, long)>();
        }
        public bool Authenticate(string key, string challengeCode)
        {
            if (ContainsKey(key))
            {
                if (vault.GetValueOrDefault(key).Item1 == challengeCode)
                {
                    return true;
                }
            }

            return false;
        }

        public long GetSubsriberId(string key)
        {
            return vault.GetValueOrDefault(key).Item2;
        }
        public bool ContainsKey(string key)
        {
            return vault.ContainsKey(key);
        }

        public void RemoveKey(string key)
        {
            vault.Remove(key);
        }

        public void New(string key, long subsriberId)
        {
            var code = GenerateChallengeCode();
            
            if (vault.ContainsKey(key)) 
            {
                RemoveKey(key);
            }
            
            vault.Add(key, (code, subsriberId));
        }

        private string GenerateChallengeCode()
        {
            var random = new Random();

            return random.Next(100000, 1000000).ToString();
        }
    }
}
