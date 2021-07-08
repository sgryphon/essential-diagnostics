using System.IO;

namespace Essential.IO
{
    /// <summary>
    /// Adapter that wraps System.IO.File and System.IO.Directory, allowing them to be substituted.
    /// </summary>
    public class FileSystem : IFileSystem
    {
        /// <summary>
        ///     Gets or sets the value indicating whether all subdirectories in full file path
        ///     should be checked for existence and re-created if missed
        ///     before opening the file. Default value is <c>False</c>.
        /// </summary>
        public bool CreateSubdirectories { get; set; }

        /// <summary>
        /// Opens a System.IO.FileStream on the specified path, 
        /// having the specified mode with read, write, or read/write access
        /// and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A value that specifies the operations that can be performed on the file.</param>
        /// <param name="share">A value specifying the type of access other threads have to the file.</param>
        /// <returns></returns>
        public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            if (CreateSubdirectories)
            {
                // Making sure that all subdirectories in file path exists
                var directory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(directory);
            }

            return File.Open(path, mode, access, share);
        }
    }
}
