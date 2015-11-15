using System;

namespace Dms.Xunit.TheoryData
{
    public class FileNotSupportedException : Exception
    {
        public FileNotSupportedException(string message) : base(message)
        {
        }
        public FileNotSupportedException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}