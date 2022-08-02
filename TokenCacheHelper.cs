using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BetterOutlookReminder.Properties;
using System.Security.Cryptography;

namespace BetterOutlookReminder
{
    static class TokenCacheHelper
    {
        public static void EnableSerialization(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
        }

        private static readonly object FileLock = new object();


        private static void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            lock (FileLock)
            {
                if (!string.IsNullOrEmpty(Settings.Default.Token))
                {
                    var bytes = Convert.FromBase64String(Settings.Default.Token);

                    args.TokenCache.DeserializeMsalV3(
                        ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser));
                }
            }
        }

        private static void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                lock (FileLock)
                {
                    var encoded = Convert.ToBase64String(
                        ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), null, DataProtectionScope.CurrentUser));

                    Settings.Default.Token = encoded;
                }
            }
        }
    }
}
