using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Exceptions
{
    public class TableAlreadyExistsException : DatabaseException
    {
        public string TableName { get; }

        public TableAlreadyExistsException(string tableName)
            : base($"Таблиця '{tableName}' вже існує в базі даних")
        {
            TableName = tableName;
        }
    }
}
