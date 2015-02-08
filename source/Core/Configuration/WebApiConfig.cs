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
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Thinktecture.IdentityManager.Configuration.Hosting;

namespace Thinktecture.IdentityManager.Configuration
{
    public class WebApiConfig
    {
        public static HttpConfiguration Configure(IdentityManagerOptions options)
        {
            if (options == null) throw new ArgumentNullException("idmConfig");

            var config = new HttpConfiguration();
            config.MessageHandlers.Insert(0, new KatanaDependencyResolver());

            config.MapHttpAttributeRoutes();
            if (!options.DisableUserInterface)
            {
                config.Routes.MapHttpRoute(Constants.RouteNames.Home,
                    "",
                    new { controller = "Page", action = "Index" });
                config.Routes.MapHttpRoute(Constants.RouteNames.OAuthFrameCallback,
                    "frame",
                    new { controller = "Page", action = "Frame" });
            }

            config.SuppressDefaultHostAuthentication();
            
            if (options.SecurityMode == SecurityMode.LocalMachine)
            {
                config.Filters.Add(new HostAuthenticationAttribute(Constants.LocalAuthenticationType));
            }
			else if (options.SecurityMode == SecurityMode.LocalAndRemote)
			{
			    config.Filters.Add(new HostAuthenticationAttribute(Constants.RemoteAuthenticationType));
			}
            else
            {
                config.Filters.Add(new HostAuthenticationAttribute(Constants.BearerAuthenticationType));
            }

            config.Filters.Add(new AuthorizeAttribute() { Roles = options.AdminRoleName });

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.Remove(config.Formatters.FormUrlEncodedFormatter);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            //apiConfig.Services.Add(typeof(IExceptionLogger), new UserAdminExceptionLogger());

//#if DEBUG
//            apiConfig.Services.Add(typeof(IExceptionLogger), new TraceLogger());
//#endif
            config.Services.Add(typeof(IExceptionLogger), new LogProviderExceptionLogger());

            return config;
        }

        //public class UserAdminExceptionLogger : ExceptionLogger
        //{
        //    public override void Log(ExceptionLoggerContext context)
        //    {
        //        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        //        path = Path.Combine(path, "UserAdminException.txt");
        //        Directory.CreateDirectory(path);
        //        var msg = DateTime.Now.ToString() + Environment.NewLine + context.Exception.ToString() + Environment.NewLine + Environment.NewLine;
        //        File.AppendAllText(path, msg);
        //    }
        //}
    }
}
