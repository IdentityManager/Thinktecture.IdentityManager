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
namespace IdentityManager
{
    public class Constants
    {
        public const string LocalAuthenticationType = "idmgr.Local";
        public const string CookieAuthenticationType = "Cookies";
        public const string BearerAuthenticationType = "Bearer";

        public const string AuthorizePath = "/authorize";
        public const string CallbackFragment = "/#/callback/";
        public const string IdMgrClientId = "idmgr";
        public const string IdMgrScope = "idmgr";
        public const string AdminRoleName = "IdentityManagerAdministrator";

        internal const string AutofacScope = "idm:AutofacScope";

        public static readonly TimeSpan DefaultTokenExpiration = TimeSpan.FromHours(10);

        public class ClaimTypes
        {
            public const string Subject = "sub";
            public const string Username = "username";
            public const string Name = "name";
            public const string Password = "password";
            public const string Email = "email";
            public const string Phone = "phone";
            public const string Role = "role";
            public const string BootstrapToken = "bootstrap-token";
        }

        public const string RoutePrefix = "api";
        public const string MetadataRoutePrefix = RoutePrefix + "";
        public const string UserRoutePrefix = RoutePrefix + "/users";
        public const string RoleRoutePrefix = RoutePrefix + "/roles";

        public class RouteNames
        {
            public const string GetUsers = "GetUsers";
            public const string GetUser = "GetUser";
            public const string CreateUser = "CreateUser";
            public const string DeleteUser = "DeleteUser";
            public const string UpdateUserProperty = "UpdateUserProperty";
            public const string AddClaim = "AddClaim";
            public const string RemoveClaim = "RemoveClaim";
            public const string AddRole = "AddRole";
            public const string RemoveRole = "RemoveRole";

            public const string GetRoles = "GetRoles";
            public const string GetRole = "GetRole";
            public const string CreateRole = "CreateRole";
            public const string DeleteRole = "DeleteRole ";
            public const string UpdateRoleProperty = "UpdateRoleProperty";

            public const string AddExternalLogin = "AddExternalLogin";
            public const string RemoveExternalLogin = "RemoveExternalLogin";

            public const string Home = "Home";
            public const string Logout = "Logout";
            public const string OAuthFrameCallback = "FrameCallback";
        }
    }
}
