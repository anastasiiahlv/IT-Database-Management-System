using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Exceptions
{
    public class ColumnNotFoundException : DatabaseException
    {
        public string ColumnName { get; }

        public ColumnNotFoundException(string columnName)
            : base($"Колонка '{columnName}' не знайдена в таблиці")
        {
            ColumnName = columnName;
        }
    }
}
