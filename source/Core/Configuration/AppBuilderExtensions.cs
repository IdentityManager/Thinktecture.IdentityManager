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
 
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.StaticFiles;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;
using Thinktecture.IdentityManager.Configuration;
using Thinktecture.IdentityManager.Configuration.Hosting;
using Thinktecture.IdentityManager.Configuration.Hosting.LocalAuthenticationMiddleware;
using Thinktecture.IdentityModel.Owin.ScopeValidation;
using Thinktecture.IdentityManager.Configuration.Hosting.RemoteAuthenticationMiddleware;

namespace Owin
{
    public static class IdentityManagerAppBuilderExtensions
    {
        public static void UseIdentityManager(this IAppBuilder app, IdentityManagerOptions options)
        {
            if (app == null) throw new ArgumentNullException("app");
            if (options == null) throw new ArgumentNullException("config");
            options.Validate();

            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            var container = AutofacConfig.Configure(options);
            app.Use<AutofacContainerMiddleware>(container);

            if (options.SecurityMode == SecurityMode.LocalMachine)
            {
                var local = new LocalAuthenticationOptions(options.AdminRoleName);
                app.Use<LocalAuthenticationMiddleware>(local);
            }
			else if(options.SecurityMode == SecurityMode.LocalAndRemote)
			{
			    var remote = new RemoteAuthenticationOptions(options.AdminRoleName);
			    app.Use<RemoteAuthenticationMiddleware>(remote);
			}
            else if (options.SecurityMode == SecurityMode.OAuth2)
            {
                var jwtParams = new System.IdentityModel.Tokens.TokenValidationParameters
                {
                    NameClaimType = options.OAuth2Configuration.NameClaimType,
                    RoleClaimType = options.OAuth2Configuration.RoleClaimType,
                    ValidAudience = options.OAuth2Configuration.Audience,
                    ValidIssuer = options.OAuth2Configuration.Issuer,
                };
                if (options.OAuth2Configuration.SigningCert != null)
                {
                    jwtParams.IssuerSigningToken = new X509SecurityToken(options.OAuth2Configuration.SigningCert);
                }
                else
                {
                    var bytes = Convert.FromBase64String(options.OAuth2Configuration.SigningKey);
                    jwtParams.IssuerSigningToken = new BinarySecretSecurityToken(bytes);
                }

                app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions {
                    TokenValidationParameters = jwtParams
                });
                app.RequireScopes(new ScopeValidationOptions {
                    AllowAnonymousAccess = true,
                    Scopes = new string[] {
                        options.OAuth2Configuration.Scope
                    }
                });
                if (options.OAuth2Configuration.ClaimsTransformation != null)
                {
                    app.Use(async (ctx, next) =>
                    {
                        var user = ctx.Authentication.User;
                        if (user != null)
                        {
                            user = options.OAuth2Configuration.ClaimsTransformation(user);
                            ctx.Authentication.User = user;
                        }

                        await next();
                    });
                }
            }

            if (!options.DisableUserInterface)
            {
                app.UseFileServer(new FileServerOptions
                {
                    RequestPath = new PathString("/assets"),
                    FileSystem = new EmbeddedResourceFileSystem(typeof(IdentityManagerAppBuilderExtensions).Assembly, "Thinktecture.IdentityManager.Assets")
                });
                app.UseFileServer(new FileServerOptions
                {
                    RequestPath = new PathString("/assets/libs/fonts"),
                    FileSystem = new EmbeddedResourceFileSystem(typeof(IdentityManagerAppBuilderExtensions).Assembly, "Thinktecture.IdentityManager.Assets.Content.fonts")
                });
                app.UseStageMarker(PipelineStage.MapHandler);
            }

            SignatureConversions.AddConversions(app);
            app.UseWebApi(WebApiConfig.Configure(options));
            app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}
