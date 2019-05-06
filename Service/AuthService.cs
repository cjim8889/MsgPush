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
    }
}
