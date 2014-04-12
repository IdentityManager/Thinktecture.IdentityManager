using Microsoft.AspNet.Identity;
using SInnovations.Identity.AzureTableStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityManager.Core;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using Microsoft.WindowsAzure.Storage.Table;

namespace AspNetIdentity.AzureTableStorage
{
        
    class TokenProvider<TUser, TKey> : IUserTokenProvider<TUser, TKey>
        where TUser : class, IUser<TKey>, new()
        where TKey : System.IEquatable<TKey>
    {
        public Task<string> GenerateAsync(string purpose, Microsoft.AspNet.Identity.UserManager<TUser, TKey> manager, TUser user)
        {
            return Task.FromResult(purpose + user.Id);
        }

        public Task<bool> IsValidProviderForUserAsync(Microsoft.AspNet.Identity.UserManager<TUser, TKey> manager, TUser user)
        {
            return Task.FromResult(true);
        }

        public Task NotifyAsync(string token, Microsoft.AspNet.Identity.UserManager<TUser, TKey> manager, TUser user)
        {
            return Task.FromResult(0);
        }

        public Task<bool> ValidateAsync(string purpose, string token, Microsoft.AspNet.Identity.UserManager<TUser, TKey> manager, TUser user)
        {
            return Task.FromResult((purpose + user.Id) == token);
        }
    }

    public class UserStore<TUser> : UserStore<TUser, IdentityRole, string, IdentityUserLogin, IdentityUserClaim>, IDisposable where TUser : IdentityUser
    {
        // Methods


        public UserStore(IdentityTableContext<TUser> context)
            : base(context)
        {
        }
    }
        //public class UserManager<TUser> : UserManager<TUser, string, IdentityUserLogin, IdentityUserRole, IdentityUserClaim>
        //    where TUser : IdentityUser, new()
        //{
        //    public UserManager(Microsoft.AspNet.Identity.UserManager<TUser> userManager, IDisposable cleanup)
        //        : base(userManager, cleanup)
        //    {
        //    }
        //}

    public class UserManager<TUser, TKey, TUserLogin, TUserRole, TUserClaim> : IUserManager, IDisposable
        where TUser : IdentityUser<TKey, TUserLogin, TUserRole, TUserClaim>, new()
        where TUserRole : IdentityRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
    //    where TUserRole : IdentityUserRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>,new()
        where TKey : IEquatable<TKey>
        {
            protected readonly Microsoft.AspNet.Identity.UserManager<TUser, TKey> userManager;
            protected readonly IdentityTableContext<TUser,TUserRole,TKey,TUserLogin,TUserClaim> tableContext;
            IDisposable cleanup;

            protected readonly Func<string, TKey> ConvertSubjectToKey;

            public UserManager(Microsoft.AspNet.Identity.UserManager<TUser, TKey> userManager, IdentityTableContext<TUser, TUserRole, TKey, TUserLogin, TUserClaim> tableContext)
            {
                if (userManager == null) throw new ArgumentNullException("userManager");

                this.userManager = userManager;
                this.tableContext = tableContext;
                this.cleanup = tableContext;

                if (userManager.UserTokenProvider == null)
                {
                    userManager.UserTokenProvider = new TokenProvider<TUser, TKey>();
                }

                var keyType = typeof(TKey);
                if (keyType == typeof(string)) ConvertSubjectToKey = subject => (TKey)ParseString(subject);
                else if (keyType == typeof(int)) ConvertSubjectToKey = subject => (TKey)ParseInt(subject);
                else if (keyType == typeof(long)) ConvertSubjectToKey = subject => (TKey)ParseLong(subject);
                else if (keyType == typeof(Guid)) ConvertSubjectToKey = subject => (TKey)ParseGuid(subject);
                else
                {
                    throw new InvalidOperationException("Key type not supported");
                }
            }

            object ParseString(string sub)
            {
                return sub;
            }
            object ParseInt(string sub)
            {
                int key;
                if (!Int32.TryParse(sub, out key)) return 0;
                return key;
            }
            object ParseLong(string sub)
            {
                long key;
                if (!Int64.TryParse(sub, out key)) return 0;
                return key;
            }
            object ParseGuid(string sub)
            {
                Guid key;
                if (!Guid.TryParse(sub, out key)) return Guid.Empty;
                return key;
            }

            public void Dispose()
            {
                if (this.cleanup != null)
                {
                    cleanup.Dispose();
                    cleanup = null;
                }
            }

            public Task<UserManagerMetadata> GetMetadataAsync()
            {
                var claims = new ClaimMetadata[]
            {
                new ClaimMetadata{
                    ClaimType = Thinktecture.IdentityManager.Core.Constants.ClaimTypes.Subject,
                    DisplayName = "Subject",
                }
            };

                return Task.FromResult(new UserManagerMetadata
                {
                    UniqueIdentitiferClaimType = Thinktecture.IdentityManager.Core.Constants.ClaimTypes.Subject,
                    Claims = claims
                });
            }

            public async Task<UserManagerResult<QueryResult>> QueryUsersAsync(string filter, int start, int count)
            {

                var query1 = (from user in tableContext.Users
                             select user);
             
                if(!string.IsNullOrEmpty(filter))
                query1 = from user in query1
                            where user.UserName.CompareTo(filter ?? "") <= 0
                            select user;

                var query = query1.AsTableQuery();
                var userlist = new List<TUser>();
               

                //TODO : Table storage do not have any support for paging other then continuationtokens.
                //     : Possible solution could be to have a PagingTable on storage that keep continue token
                //     : based on filter,start,count that could be looked up when start is not 0.
                //     : No orderby either. All this sux from the user management perspective. 
                TableQuerySegment<TUser> querySegment = null;
                while ((querySegment == null || querySegment.ContinuationToken != null) && userlist.Count < start+count)
                {
                    querySegment = await query.ExecuteSegmentedAsync(querySegment != null ? querySegment.ContinuationToken : null);
                    userlist.AddRange(querySegment.Results);
                }

                int total = userlist.Count();
                var users = userlist.Skip(start).Take(count).ToArray();

                var result = new QueryResult();
                result.Start = start;
                result.Count = count;
                result.Total = total;
                result.Filter = filter;
                result.Users = users.Select(x =>
                {
                    var user = new UserResult
                    {
                        Subject = x.Id.ToString(),
                        Username = x.UserName,
                        DisplayName = DisplayNameFromUser(x)
                    };

                    return user;
                }).ToArray();

                return new UserManagerResult<QueryResult>(result);
            }

            string DisplayNameFromUser(TUser user)
            {
                var claims = userManager.GetClaims(user.Id);
                var name = claims.Where(x => x.Type == Thinktecture.IdentityManager.Core.Constants.ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
                return name ?? user.UserName;
            }

            public async Task<UserManagerResult<UserResult>> GetUserAsync(string subject)
            {
                TKey key = ConvertSubjectToKey(subject);
                var user = await this.userManager.FindByIdAsync(key);
                if (user == null)
                {
                    return new UserManagerResult<UserResult>("Invalid subject");
                }

                var result = new UserResult
                {
                    Subject = subject,
                    Username = user.UserName,
                    DisplayName = DisplayNameFromUser(user),
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                };
                var claims = new List<Thinktecture.IdentityManager.Core.UserClaim>();
                if (user.Claims != null)
                {
                    claims.AddRange(user.Claims.Select(x => new Thinktecture.IdentityManager.Core.UserClaim { Type = x.ClaimType, Value = x.ClaimValue }));
                }
                result.Claims = claims.ToArray();

                return new UserManagerResult<UserResult>(result);
            }

            public async Task<UserManagerResult<CreateResult>> CreateUserAsync(string username, string password)
            {
                TUser user = new TUser { UserName = username };
                var result = await this.userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return new UserManagerResult<CreateResult>(new CreateResult { Subject = user.Id.ToString() });
            }

            public async Task<UserManagerResult> DeleteUserAsync(string subject)
            {
                TKey key = ConvertSubjectToKey(subject);
                var user = await this.userManager.FindByIdAsync(key);
                if (user == null)
                {
                    return new UserManagerResult("Invalid subject");
                }

                var result = await this.userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return UserManagerResult.Success;
            }

            public async Task<UserManagerResult> SetPasswordAsync(string subject, string password)
            {
                TKey key = ConvertSubjectToKey(subject);
                var token = await this.userManager.GeneratePasswordResetTokenAsync(key);
                var result = await this.userManager.ResetPasswordAsync(key, token, password);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return UserManagerResult.Success;
            }

            public async Task<UserManagerResult> SetEmailAsync(string subject, string email)
            {
                TKey key = ConvertSubjectToKey(subject);
                var result = await this.userManager.SetEmailAsync(key, email);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                var token = await this.userManager.GenerateEmailConfirmationTokenAsync(key);
                result = this.userManager.ConfirmEmail(key, token);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return UserManagerResult.Success;
            }

            public async Task<UserManagerResult> SetPhoneAsync(string subject, string phone)
            {
                TKey key = ConvertSubjectToKey(subject);
                var token = await this.userManager.GenerateChangePhoneNumberTokenAsync(key, phone);
                var result = await this.userManager.ChangePhoneNumberAsync(key, phone, token);
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return UserManagerResult.Success;
            }

            public async Task<UserManagerResult> AddClaimAsync(string subject, string type, string value)
            {
                TKey key = ConvertSubjectToKey(subject);
                var existingClaims = await userManager.GetClaimsAsync(key);
                if (!existingClaims.Any(x => x.Type == type && x.Value == value))
                {
                    var result = await this.userManager.AddClaimAsync(key, new System.Security.Claims.Claim(type, value));
                    if (!result.Succeeded)
                    {
                        return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                    }
                }

                return UserManagerResult.Success;
            }

            public async Task<UserManagerResult> DeleteClaimAsync(string subject, string type, string value)
            {
                TKey key = ConvertSubjectToKey(subject);
                var result = await this.userManager.RemoveClaimAsync(key, new System.Security.Claims.Claim(type, value));
                if (!result.Succeeded)
                {
                    return new UserManagerResult<CreateResult>(result.Errors.ToArray());
                }

                return UserManagerResult.Success;
            }
        }
    

}
