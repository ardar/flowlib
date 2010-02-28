using System.IO;

namespace FlowLib.Containers
{
    public class FileStreamContainer
    {
        public FileStream FileStream
        {
            get;
            set;
        }

        public long LastAccessed
        {
            get;
            set;
        }
    }
}
