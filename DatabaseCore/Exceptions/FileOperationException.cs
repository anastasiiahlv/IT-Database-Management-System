using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Exceptions
{
    public class FileOperationException : DatabaseException
    {
        public string FilePath { get; }

        public FileOperationException(string filePath, string message)
            : base($"Помилка роботи з файлом '{filePath}': {message}")
        {
            FilePath = filePath;
        }

        public FileOperationException(string filePath, string message, Exception innerException)
            : base($"Помилка роботи з файлом '{filePath}': {message}", innerException)
        {
            FilePath = filePath;
        }
    }
}
