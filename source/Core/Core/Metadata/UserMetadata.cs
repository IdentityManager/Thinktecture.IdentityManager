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

namespace IdentityManager
{
    public class UserMetadata
    {
        public UserMetadata()
        {
            CreateProperties = UpdateProperties = Enumerable.Empty<PropertyMetadata>();
        }

        public bool SupportsCreate { get; set; }
        public bool SupportsDelete { get; set; }
        public bool SupportsClaims { get; set; }
        public bool SupportsExternalLogins { get; set; }

        public IEnumerable<PropertyMetadata> CreateProperties { get; set; }
        public IEnumerable<PropertyMetadata> UpdateProperties { get; set; }

        internal void Validate()
        {
            if (CreateProperties == null) CreateProperties = Enumerable.Empty<PropertyMetadata>();
            foreach (var prop in CreateProperties) prop.Validate();

            var createTypes = CreateProperties.Select(x => x.Type).Distinct();
            if (createTypes.Count() < CreateProperties.Count())
            {
                var query =
                    from t in createTypes
                    let props = (from p in CreateProperties where p.Type == t select p)
                    where props.Count() > 1
                    select t;
                var names = query.Distinct().Aggregate((x, y) => x + ", " + y);
                throw new InvalidOperationException("Duplicate CreateProperties Types registered: " + names);
            }

            if (UpdateProperties == null) UpdateProperties = Enumerable.Empty<PropertyMetadata>();
            foreach (var prop in UpdateProperties) prop.Validate();

            var updateTypes = UpdateProperties.Select(x => x.Type).Distinct();
            if (updateTypes.Count() < UpdateProperties.Count())
            {
                var query =
                    from t in updateTypes
                    let props = (from p in UpdateProperties where p.Type == t select p)
                    where props.Count() > 1
                    select t;
                var names = query.Distinct().Aggregate((x, y) => x + ", " + y);
                throw new InvalidOperationException("Duplicate UpdateProperties Types registered: " + names);
            }
        }
    }
}
