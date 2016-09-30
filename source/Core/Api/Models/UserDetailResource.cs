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
using System.Web.Http.Routing;
using IdentityManager.Extensions;

namespace IdentityManager.Api.Models
{
    public class UserDetailResource
    {
        public UserDetailResource(UserDetail user, UrlHelper url, IdentityManagerMetadata idmMeta, RoleSummary[] roles)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (url == null) throw new ArgumentNullException("url");
            if (idmMeta == null) throw new ArgumentNullException("idmMeta");

            Data = new UserDetailDataResource(user, url, idmMeta, roles);

            var links = new Dictionary<string, string>();
            if (idmMeta.UserMetadata.SupportsDelete)
            {
                links["Delete"] = url.Link(Constants.RouteNames.DeleteUser, new { subject = user.Subject });
            }
            this.Links = links;
        }

        public UserDetailDataResource Data { get; set; }
        public object Links { get; set; }
    }

    public class UserDetailDataResource : Dictionary<string, object>
    {
        public UserDetailDataResource(UserDetail user, UrlHelper url, IdentityManagerMetadata meta, RoleSummary[] roles)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            this["Username"] = user.Username;
            this["Name"] = user.Name;
            this["Subject"] = user.Subject;

            if (user.Properties != null)
            {
                var props =
                    from p in user.Properties
                    let m = (from m in meta.UserMetadata.UpdateProperties where m.Type == p.Type select m).SingleOrDefault()
                    where m != null
                    select new
                    {
                        Data = m.Convert(p.Value),
                        Meta = m,
                        Links = new
                        {
                            update = url.Link(Constants.RouteNames.UpdateUserProperty,
                                new
                                {
                                    subject = user.Subject,
                                    type = p.Type.ToBase64UrlEncoded()
                                }
                            ),
                        }
                    };

                if (props.Any())
                {
                    this["Properties"] = props.ToArray();
                }
            }

            if (roles != null && user.Claims != null)
            {
                var roleClaims = user.Claims.Where(x => x.Type == meta.RoleMetadata.RoleClaimType);
                var query =
                    from r in roles
                    orderby r.Name
                    select new
                    {
                        data = roleClaims.Any(x => x.Value == r.Name),
                        meta = new
                        {
                            type = r.Name,
                            description = r.Description,
                        },
                        links = new
                        {
                            add = url.Link(Constants.RouteNames.AddRole, new { subject = user.Subject, role = r.Name.ToBase64UrlEncoded() }),
                            remove = url.Link(Constants.RouteNames.RemoveRole, new { subject = user.Subject, role = r.Name.ToBase64UrlEncoded() })
                        }
                    };
                this["Roles"] = query.ToArray();
            }

            if (meta.UserMetadata.SupportsClaims && user.Claims != null)
            {
                var claims =
                    from c in user.Claims.ToArray()
                    select new
                    {
                        Data = c,
                        Links = new
                        {
                            delete = url.Link(Constants.RouteNames.RemoveClaim, new
                            {
                                subject = user.Subject,
                                type = c.Type.ToBase64UrlEncoded(),
                                value = c.Value.ToBase64UrlEncoded()
                            })
                        }
                    };

                this["Claims"] = new
                {
                    Data = claims.ToArray(),
                    Links = new
                    {
                        create = url.Link(Constants.RouteNames.AddClaim, new { subject = user.Subject })
                    }
                };
            }


            if (meta.UserMetadata.SupportsExternalLogins && user.ExternalLogins != null)
            {
                var externalLogins =
                    from e in user.ExternalLogins.ToArray()
                    select new
                    {
                        Data = e,
                        Links = new
                        {
                            delete = url.Link(Constants.RouteNames.RemoveExternalLogin, new
                            {
                                subject = user.Subject,
                                provider = e.Provider.ToBase64UrlEncoded(),
                                providerId = e.ProviderId.ToBase64UrlEncoded()
                            })
                        }
                    };

                this["ExternalLogins"] = new
                {
                    Data = externalLogins.ToArray(),
                    Links = new
                    {
                        create = url.Link(Constants.RouteNames.AddExternalLogin, new { subject = user.Subject })
                    }
                };
            }



        }
    }
}
