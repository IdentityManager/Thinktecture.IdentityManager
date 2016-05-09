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

using Microsoft.Owin;
using Owin;
using System;

namespace IdentityManager.Configuration
{
    public abstract class SecurityConfiguration
    {
        public bool RequireSsl { get; set; }
        public string BearerAuthenticationType { get; set; }

        public string NameClaimType { get; set; }
        public string RoleClaimType { get; set; }

        public string AdminRoleName { get; set; }

        public virtual bool? ShowLoginButton { get; set; }

        protected SecurityConfiguration()
        {
            RequireSsl = true;
            BearerAuthenticationType = Constants.BearerAuthenticationType;

            NameClaimType = Constants.ClaimTypes.Name;
            RoleClaimType = Constants.ClaimTypes.Role;
            AdminRoleName = Constants.AdminRoleName;
        }

        internal virtual void Validate()
        {
            if (String.IsNullOrWhiteSpace(BearerAuthenticationType))
            {
                throw new Exception("BearerAuthenticationType is required.");
            }
            if (String.IsNullOrWhiteSpace(NameClaimType))
            {
                throw new Exception("NameClaimType is required.");
            }
            if (String.IsNullOrWhiteSpace(RoleClaimType))
            {
                throw new Exception("RoleClaimType is required.");
            }
            if (String.IsNullOrWhiteSpace(AdminRoleName))
            {
                throw new Exception("AdminRoleName is required.");
            }
        }

        public abstract void Configure(IAppBuilder app);

        internal virtual void SignOut(IOwinContext context)
        {
        }
    }
}
