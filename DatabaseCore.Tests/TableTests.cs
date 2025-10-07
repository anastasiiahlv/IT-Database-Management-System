using DatabaseCore.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Tests
{
    public class TableTests
    {
        [Fact]
        public void CreateTable_WithValidData_ShouldSucceed()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };

            // Act
            var table = new Table("TestTable", columns);

            // Assert
            table.Name.Should().Be("TestTable");
            table.Columns.Should().HaveCount(2);
            table.RowCount.Should().Be(0);
        }

        [Fact]
        public void CreateTable_WithEmptyName_ShouldThrowException()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };

            // Act & Assert
            Action act = () => new Table("", columns);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateTable_WithNoColumns_ShouldThrowException()
        {
            // Arrange
            var columns = new List<Column>();

            // Act & Assert
            Action act = () => new Table("TestTable", columns);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void CreateTable_WithDuplicateColumnNames_ShouldThrowException()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Id", DataType.String) // Дублікат
            };

            // Act & Assert
            Action act = () => new Table("TestTable", columns);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*дубльовані*");
        }

        [Fact]
        public void AddRow_WithValidData_ShouldSucceed()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            var rowData = new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Name", "Test" }
            };

            // Act
            var row = table.AddRow(rowData);

            // Assert
            table.RowCount.Should().Be(1);
            row.GetValue<int>("Id").Should().Be(1);
            row.GetValue<string>("Name").Should().Be("Test");
        }

        [Fact]
        public void UpdateRow_WithValidData_ShouldSucceed()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            var row = table.AddRow(new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Name", "Original" }
            });

            // Act
            table.UpdateRow(row.Id, new Dictionary<string, object?>
            {
                { "Name", "Updated" }
            });

            // Assert
            var updatedRow = table.GetRow(row.Id);
            updatedRow!.GetValue<string>("Name").Should().Be("Updated");
            updatedRow.GetValue<int>("Id").Should().Be(1); // Не змінилось
        }

        [Fact]
        public void DeleteRow_WithValidId_ShouldSucceed()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            var row = table.AddRow(new Dictionary<string, object?> { { "Id", 1 } });
            var rowId = row.Id;

            // Act
            var result = table.DeleteRow(rowId);

            // Assert
            result.Should().BeTrue();
            table.RowCount.Should().Be(0);
            table.GetRow(rowId).Should().BeNull();
        }

        [Fact]
        public void DeleteRow_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            // Act
            var result = table.DeleteRow(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetColumn_WithValidName_ShouldReturnColumn()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            // Act
            var column = table.GetColumn("Name");

            // Assert
            column.Should().NotBeNull();
            column!.Name.Should().Be("Name");
            column.DataType.Should().Be(DataType.String);
        }

        [Fact]
        public void GetColumn_WithInvalidName_ShouldReturnNull()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            // Act
            var column = table.GetColumn("NonExistent");

            // Assert
            column.Should().BeNull();
        }

        [Fact]
        public void Clear_ShouldRemoveAllRows()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 1 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 3 } });

            // Act
            table.Clear();

            // Assert
            table.RowCount.Should().Be(0);
            table.Rows.Should().BeEmpty();
        }

        [Fact]
        public void Table_ColumnCount_ShouldReturnCorrectValue()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Price", DataType.Money)
            };
            var table = new Table("TestTable", columns);

            // Act & Assert
            table.ColumnCount.Should().Be(3);
        }
    }
}
