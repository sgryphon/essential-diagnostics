using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Essential.Diagnostics.Tests.Utility
{
    static class ConfigUtility
    {
        public static string GetConfigDirFromTestRunDirectory(string testDir)
        {
            var configDir = testDir.Substring(0, testDir.IndexOf("TestResults"));
            if (!configDir.Contains(@"bin\Debug"))
            {
                configDir = Path.Combine(configDir, @"Essential.Diagnostics.Tests\bin\Debug");
            }
            var configPath = Path.Combine(configDir, "Essential.Diagnostics.Tests.dll.config");
            return configPath;
        }

    }
}
