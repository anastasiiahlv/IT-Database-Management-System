using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Exceptions
{
    public class TableNotFoundException : DatabaseException
    {
        public string TableName { get; }

        public TableNotFoundException(string tableName)
            : base($"Таблиця '{tableName}' не знайдена в базі даних")
        {
            TableName = tableName;
        }
    }
}
