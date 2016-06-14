using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileServer.Core
{
    public class Readers
    {
        public IFileProcessor FileProcess { get; set; }
        public IDirectoryProcessor DirectoryProcess { get; set; }
    }
}
