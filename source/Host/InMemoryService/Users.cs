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
using System.Security.Claims;

namespace IdentityManager.Host.InMemoryService
{
    public class Users
    {
        public static ICollection<InMemoryUser> Get(int random = 0)
        {
            var users = new HashSet<InMemoryUser>
            {
                new InMemoryUser{
                    Subject = Guid.Parse("081d965f-1f84-4360-90e4-8f6deac7b9bc").ToString(),
                    Username = "alice",
                    Password = "alice",
                    Email = "alice@email.com",
                    Mobile = "123",
                    Claims = new HashSet<Claim>{
                        new Claim(Constants.ClaimTypes.Name, "Alice Smith"),
                        new Claim(Constants.ClaimTypes.Role, "admin"),
                        new Claim(Constants.ClaimTypes.Role, "employee"),
                        new Claim(Constants.ClaimTypes.Role, "manager"),
                        new Claim("department", "sales"),
                    }
                },
                new InMemoryUser{
                    Subject = Guid.Parse("5f292677-d3d2-4bf9-a6f8-e982d08e1306").ToString(),
                    Username = "bob",
                    Password = "bob",
                    Email = "bob@email.com",
                    Claims = new HashSet<Claim>{
                        new Claim(Constants.ClaimTypes.Name, "Bob Smith"),
                        new Claim(Constants.ClaimTypes.Role, "employee"),
                        new Claim(Constants.ClaimTypes.Role, "developer"),
                        new Claim("department", "IT"),
                    }
                },
            };

            for (var i = 0; i < random; i++)
            {
                var user = new InMemoryUser
                {
                    Username = GenName().ToLower()
                };
                user.Claims.Add(new Claim("name", GenName() + " " + GenName()));

                user.ExternalLogins.Add(new ExternalLoginValue { Provider = "Test ExternalProvider", ProviderId = GenName() });

                users.Add(user);
            }

            return users;
        }

        private static string GenName()
        {
            var firstChar = (char)((rnd.Next(26)) + 65);
            var username = firstChar.ToString();
            for (var j = 0; j < 6; j++)
            {
                username += Char.ToLower((char)(rnd.Next(26) + 65));
            }
            return username;
        }

        static Random rnd = new Random();
    }
}