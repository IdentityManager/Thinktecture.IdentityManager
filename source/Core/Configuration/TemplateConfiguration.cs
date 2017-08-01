using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityManager.Configuration
{
    public class AssetConfiguration
    {
        public string HostedAssetRelativePath { get; set; }

        public string HostedAssetRootFullPath { get; set;}

        public bool HostClientSideAssetDirectories { get; set; }

        public AssetMap[] ReplacementAssetStaticFileMap  { get; set; }
    }

    public class AssetMap
    {
        public string EmbeddedAssetName { get; set; }
        public string HostedRelativePath { get; set; }
    }
}
