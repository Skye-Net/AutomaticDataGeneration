using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutomaticDataGeneration.Extensions
{
    public static class FileExtensions
    {
        public static IEnumerable<string> FilterFiles(string path, params string[] exts) {
            return 
                exts.Select(x => "*." + x) // turn into globs
                    .SelectMany(x => 
                        Directory.GetFiles(path, x)
                    );
        }
    }
}