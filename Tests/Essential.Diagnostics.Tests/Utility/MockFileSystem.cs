using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Essential.IO;

namespace Essential.Diagnostics.Tests.Utility
{
    class MockFileSystem : IFileSystem
    {
        public IList<Tuple<string,MemoryStream>> OpenedItems = new List<Tuple<string,MemoryStream>>();

        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            var tuple = new Tuple<string,MemoryStream>(path, new MemoryStream());
            OpenedItems.Add(tuple);
            return tuple.Item2;
        }
    }
}
