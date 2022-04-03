using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp
{
    internal class SharedMemory
    {
        private string physicsMap = "Local\\acpmf_physics";
        private string graphicsMap = "Local\\acpmf_graphics";
        private string staticMap = "Local\\acpmf_static";


        public SharedMemory()
        {
            var mappedFile = MemoryMappedFile.OpenExisting(physicsMap, MemoryMappedFileRights.Read);

            using (Stream view = mappedFile.CreateViewStream())
            {
                //read the stream here.
            }
        }
    }
}
