using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Installer;
internal class DirectoryExtensions
{
    public static bool ExistOrCreate(string path)
    {
        bool exists = Directory.Exists(path);
        if (!exists) Directory.CreateDirectory(path);
        return exists;
    }
}
