using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FamilyBudget.data
{
    public static class PasswordHelper
    {
        public static string GenerateSalt()
        {
            RNGCryptoServiceProvider providor = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[10];
            providor.GetBytes(bytes);
            return Convert.ToBase64String(bytes);

        }

        public static string HashPassword(string password, string salt)
        {
            SHA256Managed crypt = new SHA256Managed();
            string CombinedString = password + salt;
            byte[] Combined = Encoding.Unicode.GetBytes(CombinedString);

            byte[] Hash = crypt.ComputeHash(Combined);
            return Convert.ToBase64String(Hash);
        }

        public static bool PasswordHash(string userInput, string salt, string passwordHash)
        {
            string UserInputHash = HashPassword(userInput, salt);
            return UserInputHash == passwordHash;
        }

    }
}
