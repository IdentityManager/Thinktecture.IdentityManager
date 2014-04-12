/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */

using AspNetIdentity.AzureTableStorage;
using Owin;

namespace Thinktecture.IdentityManager.Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIdentityManager(new IdentityManagerConfiguration()
            {
                //UserManagerFactory = Thinktecture.IdentityManager.MembershipReboot.UserManagerFactory.Create
                //UserManagerFactory = Thinktecture.IdentityManager.AspNetIdentity.UserManagerFactory.Create
                UserManagerFactory = UserManagerFactory.Create
            });
        }
    }

    
}