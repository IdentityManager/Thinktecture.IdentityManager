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
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityManager.Extensions;

namespace IdentityManager.Host.InMemoryService
{
    class InMemoryIdentityManagerService : IIdentityManagerService
    {
        ICollection<InMemoryUser> users;
        ICollection<InMemoryRole> roles;
        public InMemoryIdentityManagerService(ICollection<InMemoryUser> users, ICollection<InMemoryRole> roles)
        {
            this.users = users;
            this.roles = roles;
        }


        IdentityManagerMetadata metadata;

        IdentityManagerMetadata GetMetadata()
        {
            if (metadata == null)
            {
                var createprops = new List<PropertyMetadata>()
                {
                    PropertyMetadata.FromProperty<InMemoryUser>(x => x.Username, type:Constants.ClaimTypes.Username, required:true),
                };

                var updateprops = new List<PropertyMetadata>();
                updateprops.AddRange(new PropertyMetadata[]{
                    PropertyMetadata.FromProperty<InMemoryUser>(x => x.Username, type:Constants.ClaimTypes.Username, required:true),
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Password", type:Constants.ClaimTypes.Password, required:true),
                    PropertyMetadata.FromFunctions<InMemoryUser, string>(Constants.ClaimTypes.Name, this.GetName, this.SetName, name:"Name", required:true),
                });
                updateprops.AddRange(PropertyMetadata.FromType<InMemoryUser>());
                updateprops.AddRange(new PropertyMetadata[]{
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Mobile"),
                    PropertyMetadata.FromPropertyName<InMemoryUser>("Email", dataType:PropertyDataType.Email),
                    new PropertyMetadata {
                        Name = "Is Administrator",
                        Type = "role.admin",
                        DataType = PropertyDataType.Boolean,
                        Required = true,
                    },
                    new PropertyMetadata {
                        Name = "Gravatar Url",
                        Type = "gravatar",
                        DataType = PropertyDataType.Url,
                    }
                });

                var roleCreateProps = new List<PropertyMetadata>();
                roleCreateProps.Add(PropertyMetadata.FromProperty<InMemoryRole>(x => x.Name));
                var roleUpdateProps = new List<PropertyMetadata>();
                roleUpdateProps.Add(PropertyMetadata.FromProperty<InMemoryRole>(x=>x.Description, type:"description"));

                metadata = new IdentityManagerMetadata()
                {
                    UserMetadata = new UserMetadata
                    {
                        SupportsCreate = true,
                        SupportsDelete = true,
                        SupportsClaims = true,
                        SupportsExternalLogins = true,
                        CreateProperties = createprops,
                        UpdateProperties = updateprops
                    },
                    RoleMetadata = new RoleMetadata
                    {
                        RoleClaimType = Constants.ClaimTypes.Role,
                        SupportsCreate = true,
                        SupportsDelete = true,
                        CreateProperties = roleCreateProps,
                        UpdateProperties = roleUpdateProps
                    }
                };
            }
            return metadata;
        }

        #region Users
        private string GetName(InMemoryUser user)
        {
            return user.Claims.GetValue(Constants.ClaimTypes.Name);
        }

        private IdentityManagerResult SetName(InMemoryUser user, string value)
        {
            user.Claims.SetValue(Constants.ClaimTypes.Name, value);
            return IdentityManagerResult.Success;
        }

        public System.Threading.Tasks.Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            return Task.FromResult(GetMetadata());
        }

        public System.Threading.Tasks.Task<IdentityManagerResult<CreateResult>> CreateUserAsync(IEnumerable<PropertyValue> properties)
        {
            var errors = ValidateUserProperties(properties);
            if (errors.Any())
            {
                return Task.FromResult(new IdentityManagerResult<CreateResult>(errors.ToArray()));
            }

            var user = new InMemoryUser();
            var createPropsMeta = GetMetadata().UserMetadata.GetCreateProperties();
            foreach(var prop in properties)
            {
                var result = SetUserProperty(createPropsMeta, user, prop.Type, prop.Value);
                if (!result.IsSuccess)
                {
                    return Task.FromResult(new IdentityManagerResult<CreateResult>(result.Errors.ToArray()));
                }
            }
            
            users.Add(user);

            return Task.FromResult(new IdentityManagerResult<CreateResult>(new CreateResult() { Subject = user.Subject }));
        }

        public System.Threading.Tasks.Task<IdentityManagerResult> DeleteUserAsync(string subject)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user != null)
            {
                users.Remove(user);
            }
            return Task.FromResult(IdentityManagerResult.Success);
        }

        public System.Threading.Tasks.Task<IdentityManagerResult<QueryResult<UserSummary>>> QueryUsersAsync(string filter, int start, int count)
        {
            var query =
                from u in users
                select u;
            if (!String.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query =
                    from u in query
                    let names = (from c in u.Claims where c.Type == Constants.ClaimTypes.Name select c.Value.ToLower())
                    where
                        u.Username.ToLower().Contains(filter) ||
                        names.Contains(filter)
                    select u;
            }

            var items =
                from u in query.Distinct()
                select new UserSummary
                {
                    Subject = u.Subject,
                    Username = u.Username,
                    Name = u.Claims.Where(x => x.Type == Constants.ClaimTypes.Name).Select(x => x.Value).FirstOrDefault(),
                };
            var total = items.Count();

            var result = items.Skip(start).Take(count);
            return Task.FromResult(new IdentityManagerResult<QueryResult<UserSummary>>(new QueryResult<UserSummary>
            {
                Filter = filter,
                Start = start,
                Count = result.Count(),
                Items = result,
                Total = total,
            }));
        }

        public System.Threading.Tasks.Task<IdentityManagerResult<UserDetail>> GetUserAsync(string subject)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult(new IdentityManagerResult<UserDetail>((UserDetail)null));
            }

            var props = new List<PropertyValue>();
            foreach(var prop in GetMetadata().UserMetadata.UpdateProperties)
            {
                props.Add(new PropertyValue{
                    Type = prop.Type, 
                    Value = GetUserProperty(prop, user)
                });
            }

            var claims = user.Claims.Select(x => new ClaimValue { Type = x.Type, Value = x.Value });

            return Task.FromResult(new IdentityManagerResult<UserDetail>(new UserDetail
            {
                Subject = user.Subject,
                Username = user.Username,
                Name = user.Claims.GetValue(Constants.ClaimTypes.Name),
                Properties = props,
                Claims = claims
            }));
        }

        public System.Threading.Tasks.Task<IdentityManagerResult> SetUserPropertyAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult(new IdentityManagerResult("No user found"));
            }

            var errors = ValidateUserProperty(type, value);
            if (errors.Any())
            {
                return Task.FromResult(new IdentityManagerResult(errors.ToArray()));
            }

            var result = SetUserProperty(GetMetadata().UserMetadata.UpdateProperties, user, type, value);
            return Task.FromResult(result);
        }

        public System.Threading.Tasks.Task<IdentityManagerResult> AddUserClaimAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult(new IdentityManagerResult("No user found"));
            }

            user.Claims.AddClaim(type, value);
            
            return Task.FromResult(IdentityManagerResult.Success);
        }

        public System.Threading.Tasks.Task<IdentityManagerResult> RemoveUserClaimAsync(string subject, string type, string value)
        {
            var user = users.SingleOrDefault(x => x.Subject == subject);
            if (user == null)
            {
                return Task.FromResult(new IdentityManagerResult("No user found"));
            }

            user.Claims.RemoveClaims(type, value);

            return Task.FromResult(IdentityManagerResult.Success);
        }

        private string GetUserProperty(PropertyMetadata property, InMemoryUser user)
        {
            string value;
            if (property.TryGet(user, out value))
            {
                return value;
            }
            
            switch (property.Type)
            {
                case "role.admin":
                    return user.Claims.HasValue(Constants.ClaimTypes.Role, "admin").ToString().ToLower();
                case "gravatar":
                    return user.Claims.GetValue("gravatar");
            }
           
            throw new Exception("Invalid property type " + property.Type);
        }

        private IdentityManagerResult SetUserProperty(IEnumerable<PropertyMetadata> propsMeta, InMemoryUser user, string type, string value)
        {
            IdentityManagerResult result;
            if (propsMeta.TrySet(user, type, value, out result))
            {
                return result;
            }

            switch (type)
            {
                case "role.admin":
                    {
                        var val = Boolean.Parse(value);
                        if (val)
                        {
                            user.Claims.AddClaim(Constants.ClaimTypes.Role, "admin");
                        }
                        else
                        {
                            user.Claims.RemoveClaim(Constants.ClaimTypes.Role, "admin");
                        }
                    }
                    break;
                case "gravatar":
                    {
                        user.Claims.SetValue("gravatar", value);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Invalid Property Type : " + type);
            }

            return IdentityManagerResult.Success;
        }

        IEnumerable<string> ValidateUserProperties(IEnumerable<PropertyValue> properties)
        {
            return properties.Select(x => ValidateUserProperty(x.Type, x.Value)).Aggregate((x, y) => x.Concat(y));
        }

        private IEnumerable<string> ValidateUserProperty(string type, string value)
        {
            switch (type)
            {
                case Constants.ClaimTypes.Username:
                    {
                        if (this.users.Any(x => x.Username == value))
                        {
                            return new string[] { "That Username is already in use" };
                        }
                    }
                    break;
                case Constants.ClaimTypes.Password:
                    {
                        if (value.Length < 3)
                        {
                            return new string[] { "Password must have at least 3 characters" };
                        }
                    }
                    break;
            }
            
            return Enumerable.Empty<string>();
        }
        
#endregion
        
        #region Roles

        public Task<IdentityManagerResult<CreateResult>> CreateRoleAsync(IEnumerable<PropertyValue> properties)
        {
            var errors = ValidateRoleProperties(properties);
            if (errors.Any())
            {
                return Task.FromResult(new IdentityManagerResult<CreateResult>(errors.ToArray()));
            }

            var role = new InMemoryRole();
            var createPropsMeta = GetMetadata().RoleMetadata.GetCreateProperties();
            foreach (var prop in properties)
            {
                var result = SetRoleProperty(createPropsMeta, role, prop.Type, prop.Value);
                if (!result.IsSuccess)
                {
                    return Task.FromResult(new IdentityManagerResult<CreateResult>(result.Errors.ToArray()));
                }
            }

            if (roles.Any(x=>x.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return Task.FromResult(new IdentityManagerResult<CreateResult>("Role name already in use."));
            }

            roles.Add(role);

            return Task.FromResult(new IdentityManagerResult<CreateResult>(new CreateResult() { Subject = role.ID }));
        }

        public Task<IdentityManagerResult> DeleteRoleAsync(string subject)
        {
            var role = roles.SingleOrDefault(x => x.ID == subject);
            if (role != null)
            {
                roles.Remove(role);
            }

            return Task.FromResult(IdentityManagerResult.Success);
        }
        
        public Task<IdentityManagerResult<QueryResult<RoleSummary>>> QueryRolesAsync(string filter, int start, int count)
        {
            var query =
                from r in roles
                select r;
            if (!String.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                query =
                    from r in query
                    where
                        r.Name.ToLower().Contains(filter) ||
                        (r.Description != null && r.Description.ToLower().Contains(filter))
                    select r;
            }

            var items =
                from r in query.Distinct()
                select new RoleSummary
                {
                    Subject = r.ID,
                    Name = r.Name,
                    Description = r.Description
                };
            var total = items.Count();

            var result = items;
            if (start >= 0 && count >= 0)
            {
                result = items.Skip(start).Take(count);
                count = result.Count();
            }
            else
            {
                start = 0;
                count = total;
            }
            
            return Task.FromResult(new IdentityManagerResult<QueryResult<RoleSummary>>(new QueryResult<RoleSummary>
            {
                Filter = filter,
                Start = start,
                Count = count,
                Items = result,
                Total = total,
            }));
        }

        public Task<IdentityManagerResult<RoleDetail>> GetRoleAsync(string subject)
        {
            var role = this.roles.SingleOrDefault(x => x.ID == subject);
            if (role == null)
            {
                return Task.FromResult(new IdentityManagerResult<RoleDetail>((RoleDetail)null));
            }

            var props = new List<PropertyValue>();
            foreach (var prop in GetMetadata().RoleMetadata.UpdateProperties)
            {
                props.Add(new PropertyValue
                {
                    Type = prop.Type,
                    Value = GetRoleProperty(prop, role)
                });
            }

            var detail = new RoleDetail
            {
                Subject = role.ID,
                Name = role.Name,
                Description = role.Description,
                Properties = props
            };
            
            return Task.FromResult(new IdentityManagerResult<RoleDetail>(detail));
        }

        public Task<IdentityManagerResult> SetRolePropertyAsync(string subject, string type, string value)
        {
            var role = roles.SingleOrDefault(x => x.ID == subject);
            if (role == null)
            {
                return Task.FromResult(new IdentityManagerResult("No role found"));
            }

            var errors = ValidateRoleProperty(type, value);
            if (errors.Any())
            {
                return Task.FromResult(new IdentityManagerResult(errors.ToArray()));
            }

            var result = SetRoleProperty(GetMetadata().RoleMetadata.UpdateProperties, role, type, value);
            return Task.FromResult(result);
        }

        public Task<IdentityManagerResult> AddUserExternalLoginAsync(string subject, string provider, string providerId)
        {
            // @todo(PQP)
            throw new NotImplementedException();
        }

        public Task<IdentityManagerResult> RemoveUserExternalLoginAsync(string subject, string provider, string providerId)
        {
            // @todo(PQP)
            throw new NotImplementedException();
        }

        private string GetRoleProperty(PropertyMetadata property, InMemoryRole role)
        {
            string value;
            if (property.TryGet(role, out value))
            {
                return value;
            }

            throw new Exception("Invalid property type " + property.Type);
        }

        private IdentityManagerResult SetRoleProperty(IEnumerable<PropertyMetadata> propsMeta, InMemoryRole role, string type, string value)
        {
            IdentityManagerResult result;
            if (propsMeta.TrySet(role, type, value, out result))
            {
                return result;
            }

            throw new InvalidOperationException("Invalid Property Type : " + type);
        }

        IEnumerable<string> ValidateRoleProperties(IEnumerable<PropertyValue> properties)
        {
            return properties.Select(x => ValidateRoleProperty(x.Type, x.Value)).Aggregate((x, y) => x.Concat(y));
        }

        private IEnumerable<string> ValidateRoleProperty(string type, string value)
        {

            return Enumerable.Empty<string>();
        }
        
        #endregion
    }
}
