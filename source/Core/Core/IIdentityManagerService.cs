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
 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityManager
{
    public interface IIdentityManagerService
    {
        Task<IdentityManagerMetadata> GetMetadataAsync();
        
        // users
        Task<IdentityManagerResult<CreateResult>> CreateUserAsync(IEnumerable<PropertyValue> properties);
        Task<IdentityManagerResult> DeleteUserAsync(string subject);
        
        Task<IdentityManagerResult<QueryResult<UserSummary>>> QueryUsersAsync(string filter, int start, int count);
        Task<IdentityManagerResult<UserDetail>> GetUserAsync(string subject);

        Task<IdentityManagerResult> SetUserPropertyAsync(string subject, string type, string value);
        
        Task<IdentityManagerResult> AddUserClaimAsync(string subject, string type, string value);
        Task<IdentityManagerResult> RemoveUserClaimAsync(string subject, string type, string value);

        // roles
        Task<IdentityManagerResult<CreateResult>> CreateRoleAsync(IEnumerable<PropertyValue> properties);
        Task<IdentityManagerResult> DeleteRoleAsync(string subject);

        Task<IdentityManagerResult<QueryResult<RoleSummary>>> QueryRolesAsync(string filter, int start, int count);
        Task<IdentityManagerResult<RoleDetail>> GetRoleAsync(string subject);

        Task<IdentityManagerResult> SetRolePropertyAsync(string subject, string type, string value);

        // external logins
        Task<IdentityManagerResult> AddUserExternalLoginAsync(string subject, string provider, string providerId);
        Task<IdentityManagerResult> RemoveUserExternalLoginAsync(string subject, string provider, string providerId);
    }
}
