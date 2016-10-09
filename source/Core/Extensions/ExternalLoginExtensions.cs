/*
 * Copyright 2014 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityManager.Extensions
{
    public static class ExternalLoginExtensions
    {
        public static bool HasValue(this IEnumerable<ExternalLoginValue> externalLogins, string provider, string providerId)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(providerId)) throw new ArgumentNullException("providerId");

            return externalLogins.Any(x => x.Provider == provider && x.ProviderId == providerId);
        }

        public static bool HasExternalLoginValue(this IEnumerable<ExternalLoginValue> externalLogins, string provider)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            return externalLogins.Any(x => x.Provider == provider);
        }

        public static void AddExternalLogin(this ICollection<ExternalLoginValue> externalLogins, string provider, string providerId)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(providerId)) throw new ArgumentNullException("providerId");

            if (!externalLogins.HasValue(provider, providerId))
            {
                externalLogins.Add(new ExternalLoginValue { Provider = provider, ProviderId = providerId} );
            }
        }

        public static void RemoveExternalLogin(this ICollection<ExternalLoginValue> externalLogins, string provider, string providerId)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(providerId)) throw new ArgumentNullException("providerId");

            foreach (var externalLogin in externalLogins.Where(x => x.Provider == provider && x.ProviderId == providerId).ToArray())
            {
                externalLogins.Remove(externalLogin);
            }
        }

        public static void RemoveExternalLogins(this ICollection<ExternalLoginValue> externalLogins, string provider)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");

            var found = externalLogins.Where(x => x.Provider == provider).ToArray();
            foreach (var externalLogin in found)
            {
                externalLogins.Remove(externalLogin);
            }
        }

        public static void RemoveExternalLogins(this ICollection<ExternalLoginValue> externalLogins, string provider, string providerId)
        {
            if (externalLogins == null) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(provider)) throw new ArgumentNullException("provider");
            if (String.IsNullOrWhiteSpace(providerId)) throw new ArgumentNullException("providerId");

            var found = externalLogins.Where(x => x.Provider == provider && x.ProviderId == providerId).ToArray();
            foreach (var externalLogin in found)
            {
                externalLogins.Remove(externalLogin);
            }
        }
    }
}
