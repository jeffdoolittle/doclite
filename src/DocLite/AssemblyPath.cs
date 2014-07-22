using System;
using System.IO;
using System.Reflection;

namespace DocLite
{
    public static class AssemblyPath
    {
        public static string ExecutingAssembly
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                var directory = Path.GetDirectoryName(path);
                return directory;
            }
        }
    }
}