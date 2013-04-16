using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Essential.Diagnostics.Tests.Utility
{
    static class ConfigUtility
    {
        public static string GetConfigDirFromTestRunDirectory(string testDir)
        {
//            var configDir = testDir.Substring(0, testDir.IndexOf("TestResults"));
//#if DEBUG
//            var expectedTargetDir = @"bin\Debug";
//#else
//            var expectedTargetDir = @"bin\Release";
//#endif

//            if (!configDir.Contains(expectedTargetDir))
//            {
//                configDir = Path.Combine(configDir, @"Essential.Diagnostics.Tests\" + expectedTargetDir);
//            }
//            var configPath = Path.Combine(configDir, "Essential.Diagnostics.Tests.dll.config");
            //return Path.Combine( testDir + "\\Out", "Essential.Diagnostics.Tests.dll.config");
            return Assembly.GetExecutingAssembly().Location + ".config";
        }

    }
}
