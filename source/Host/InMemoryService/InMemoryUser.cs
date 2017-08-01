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
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IdentityManager.Host.InMemoryService
{
    public class InMemoryUserBase
    {
        public InMemoryUserBase()
        {
            Claims = new HashSet<Claim>();
            ExternalLogins = new List<ExternalLoginValue>();
            Subject = Guid.NewGuid().ToString();
        }

        public string Subject { get; set; }

        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Mobile { get; set; }

        public ICollection<Claim> Claims { get; set; }

        public ICollection<ExternalLoginValue> ExternalLogins { get; set; }
    }

    public class InMemoryUser : InMemoryUserBase
    {
        [Display(Name="First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool IsNice { get; set; }
    }
}