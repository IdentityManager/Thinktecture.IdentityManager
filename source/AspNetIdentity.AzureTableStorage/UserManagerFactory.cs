using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using SInnovations.Identity.AzureTableStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityManager.Core;

namespace AspNetIdentity.AzureTableStorage
{
    public class UserManagerFactory
    {

        public static IUserManager Create()
        {
            //Either paste in your key here or just put into a file and avoid committing it to github.
            var db = new IdentityTableContext<IdentityUser>(new CloudStorageAccount(
                new StorageCredentials("c1azuretests", File.ReadAllText("C:\\dev\\storagekey.txt")), true));
            
            var store = new UserStore<IdentityUser>(db);
            var mgr = new Microsoft.AspNet.Identity.UserManager<IdentityUser,string>(store);

            return new UserManager<IdentityUser,string,IdentityUserLogin,IdentityRole,IdentityUserClaim>(mgr, db);

            //var db = new CustomDbContext("CustomAspId");
            //var store = new CustomUserStore(db);
            //var mgr = new CustomUserManager(store);
            //return new Thinktecture.IdentityManager.AspNetIdentity.UserManager<CustomUser, int, CustomUserLogin, CustomUserRole, CustomUserClaim>(mgr, db);
        }
    }
}
