using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Exceptions
{
    public class RowNotFoundException : DatabaseException
    {
        public Guid RowId { get; }

        public RowNotFoundException(Guid rowId)
            : base($"Рядок з ID '{rowId}' не знайдено")
        {
            RowId = rowId;
        }
    }
}
