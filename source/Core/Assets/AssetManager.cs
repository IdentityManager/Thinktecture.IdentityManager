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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using IdentityManager.Configuration;

namespace IdentityManager.Assets
{
    class AssetManager
    {
        static ConcurrentDictionary<string, string> ResourceStrings = new ConcurrentDictionary<string, string>();
        internal static string LoadResourceString(string name, AssetConfiguration config)
        {
            string value;
            if (!ResourceStrings.TryGetValue(name, out value))
            {
                var replacementFileMap = config.ReplacementAssetStaticFileMap.First(x => string.Equals(x.EmbeddedAssetName, name, StringComparison.CurrentCultureIgnoreCase));
                var replacementFilePath = (replacementFileMap != null) ? Path.Combine(config.HostedAssetRootFullPath,replacementFileMap.HostedRelativePath) : null;
                
                if (!string.IsNullOrWhiteSpace(replacementFilePath) && File.Exists(new Uri(replacementFilePath).LocalPath))
                {
                    value = File.ReadAllText(replacementFilePath);
                }
                else
                {
                    var assembly = typeof(AssetManager).Assembly;
                    using (var sr = new StreamReader(assembly.GetManifestResourceStream(name)))
                    {
                        value = sr.ReadToEnd();
                    }
                }
                ResourceStrings[name] = value;
            }
            return value;
        }

        internal static string LoadResourceString(string name, AssetConfiguration config, IDictionary<string, object> values)
        {
            string value = LoadResourceString(name, config);
            foreach(var key in values.Keys)
            {
                var val = values[key];
                value = value.Replace("{" + key + "}",  val != null ? val.ToString() : "");
            }
            return value;
        }
        
        internal static string LoadResourceString(string name, AssetConfiguration config,  object values)
        {
            return LoadResourceString(name, config, Map(values));
        }

        static IDictionary<string, object> Map(object values)
        {
            var dictionary = values as IDictionary<string, object>;
            
            if (dictionary == null) 
            {
                dictionary = new Dictionary<string, object>();
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(values))
                {
                    dictionary.Add(descriptor.Name, descriptor.GetValue(values));
                }
            }

            return dictionary;
        }
    }
}

