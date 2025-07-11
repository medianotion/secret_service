using System;

namespace Security
{
    internal static class Helpers
    {
        internal static void ValidateSecretKey(string secretKey)
        {

            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentNullException("secretKey", "secretKey value is null or empty");
        }

    }
}
