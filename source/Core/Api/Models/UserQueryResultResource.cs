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
using System.Web.Http.Routing;
using AutoMapper;

namespace IdentityManager.Api.Models
{
    public class UserQueryResultResource
    {
        public UserQueryResultResource(QueryResult<UserSummary> result, UrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Data = new UserQueryResultResourceData(result, url, meta);
            
            var links = new Dictionary<string, object>();
            if (meta.SupportsCreate)
            {
                links["create"] = new CreateUserLink(url, meta);
            };
            Links = links;
        }

        public UserQueryResultResourceData Data { get; set; }
        public object Links { get; set; }
    }

    public class UserQueryResultResourceData : QueryResult<UserSummary>
    {
        public static IMapper Mapper { get; set; }
        static UserQueryResultResourceData()
        {
            Mapper = new MapperConfiguration(config =>
            {
                config.CreateMap<QueryResult<UserSummary>, UserQueryResultResourceData>()
                    .ForMember(x => x.Items, opts => opts.MapFrom(x => x.Items));
                config.CreateMap<UserSummary, UserResultResource>()
                    .ForMember(x => x.Data, opts => opts.MapFrom(x => x));
            }).CreateMapper();
        }

        public UserQueryResultResourceData(QueryResult<UserSummary> result, UrlHelper url, UserMetadata meta)
        {
            if (result == null) throw new ArgumentNullException("result");
            if (url == null) throw new ArgumentNullException("url");
            if (meta == null) throw new ArgumentNullException("meta");

            Mapper.Map(result, this);

            foreach (var user in this.Items)
            {
                var links = new Dictionary<string, string> {
                    {"Detail", url.Link(Constants.RouteNames.GetUser, new { subject = user.Data.Subject })}
                };
                if (meta.SupportsDelete)
                {
                    links.Add("delete", url.Link(Constants.RouteNames.DeleteUser, new { subject = user.Data.Subject }));
                }
                user.Links = links;
            }
        }

        public new IEnumerable<UserResultResource> Items { get; set; }
    }

    public class UserResultResource
    {
        public UserSummary Data { get; set; }
        public object Links { get; set; }
    }
}
