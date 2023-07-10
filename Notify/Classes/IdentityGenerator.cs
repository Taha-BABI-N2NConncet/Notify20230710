using Notify.Interfaces;
using System;

namespace Notify.Classes
{
    public class IdentityGenerator : IIdentityGenerator
    {
        private Random random = new Random();

        public string GetIDentityString(int length = 4)
        {
            return $"{DateTime.Now.ToString("yyyyMMssHHmmss")}{GetRandomString(length)}";
        }

        public string GetRandomString(int length)
        {
            const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
