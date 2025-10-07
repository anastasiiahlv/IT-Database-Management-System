using DatabaseCore.Exceptions;
using DatabaseCore.Managers;
using DatabaseCore.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Tests
{
    public class DatabaseManagerTests : IDisposable
    {
        private readonly DatabaseManager _manager;
        private readonly string _testFilePath;

        public DatabaseManagerTests()
        {
            _manager = new DatabaseManager();
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        }

        public void Dispose()
        {
            // Видаляємо тестовий файл після кожного тесту
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        [Fact]
        public void CreateDatabase_WithValidName_ShouldSucceed()
        {
            // Act
            var database = _manager.CreateDatabase("TestDatabase");

            // Assert
            database.Should().NotBeNull();
            database.Name.Should().Be("TestDatabase");
            _manager.HasOpenDatabase.Should().BeTrue();
            _manager.CurrentDatabase.Should().Be(database);
        }

        [Fact]
        public void CreateDatabase_WithEmptyName_ShouldThrowException()
        {
            // Act & Assert
            Action act = () => _manager.CreateDatabase("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SaveDatabase_WithValidPath_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");

            // Act
            Action act = () => _manager.SaveDatabase(_testFilePath);

            // Assert
            act.Should().NotThrow();
            File.Exists(_testFilePath).Should().BeTrue();
            _manager.CurrentFilePath.Should().Be(_testFilePath);
        }

        [Fact]
        public void SaveDatabase_WithoutOpenDatabase_ShouldThrowException()
        {
            // Act & Assert
            Action act = () => _manager.SaveDatabase(_testFilePath);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Немає відкритої бази даних*");
        }

        [Fact]
        public void LoadDatabase_WithValidFile_ShouldSucceed()
        {
            // Arrange - Створюємо та зберігаємо базу
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            _manager.CreateTable("TestTable", columns);
            _manager.SaveDatabase(_testFilePath);

            // Закриваємо базу
            _manager.CloseDatabase();

            // Act - Завантажуємо базу
            var loadedDatabase = _manager.LoadDatabase(_testFilePath);

            // Assert
            loadedDatabase.Should().NotBeNull();
            loadedDatabase.Name.Should().Be("TestDatabase");
            loadedDatabase.TableCount.Should().Be(1);
            loadedDatabase.TableExists("TestTable").Should().BeTrue();
        }

        [Fact]
        public void LoadDatabase_WithNonExistentFile_ShouldThrowException()
        {
            // Act & Assert
            Action act = () => _manager.LoadDatabase("nonexistent_file.json");
            act.Should().Throw<FileOperationException>();
        }

        [Fact]
        public void CreateTable_WithValidData_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Price", DataType.Money)
            };

            // Act
            var table = _manager.CreateTable("Products", columns);

            // Assert
            table.Should().NotBeNull();
            table.Name.Should().Be("Products");
            table.ColumnCount.Should().Be(3);
            _manager.GetTableNames().Should().Contain("Products");
        }

        [Fact]
        public void CreateTable_WithInvalidName_ShouldThrowException()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };

            // Act & Assert
            Action act = () => _manager.CreateTable("Invalid-Name", columns); // Містить дефіс
            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void CreateTable_WithoutOpenDatabase_ShouldThrowException()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };

            // Act & Assert
            Action act = () => _manager.CreateTable("TestTable", columns);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Немає відкритої бази даних*");
        }

        [Fact]
        public void DeleteTable_WithExistingTable_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column> { new Column("Id", DataType.Integer) };
            _manager.CreateTable("TestTable", columns);

            // Act
            var result = _manager.DeleteTable("TestTable");

            // Assert
            result.Should().BeTrue();
            _manager.GetTableNames().Should().NotContain("TestTable");
        }

        [Fact]
        public void DeleteTable_WithNonExistentTable_ShouldReturnFalse()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");

            // Act
            var result = _manager.DeleteTable("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void AddRow_WithValidData_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            _manager.CreateTable("TestTable", columns);

            var rowData = new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Name", "Test" }
            };

            // Act
            var row = _manager.AddRow("TestTable", rowData);

            // Assert
            row.Should().NotBeNull();
            row.GetValue<int>("Id").Should().Be(1);
            row.GetValue<string>("Name").Should().Be("Test");
        }

        [Fact]
        public void AddRow_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            _manager.CreateTable("TestTable", columns);

            var rowData = new Dictionary<string, object?>
            {
                { "Id", "not_an_integer" } // Невалідний тип
            };

            // Act & Assert
            Action act = () => _manager.AddRow("TestTable", rowData);
            act.Should().Throw<ValidationException>();
        }

        [Fact]
        public void UpdateRow_WithValidData_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            _manager.CreateTable("TestTable", columns);

            var row = _manager.AddRow("TestTable", new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Name", "Original" }
            });

            // Act
            _manager.UpdateRow("TestTable", row.Id, new Dictionary<string, object?>
            {
                { "Name", "Updated" }
            });

            // Assert
            var updatedRow = _manager.GetRow("TestTable", row.Id);
            updatedRow.GetValue<string>("Name").Should().Be("Updated");
        }

        [Fact]
        public void DeleteRow_WithValidId_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column> { new Column("Id", DataType.Integer) };
            _manager.CreateTable("TestTable", columns);

            var row = _manager.AddRow("TestTable", new Dictionary<string, object?> { { "Id", 1 } });

            // Act
            var result = _manager.DeleteRow("TestTable", row.Id);

            // Assert
            result.Should().BeTrue();
            var table = _manager.GetTable("TestTable");
            table.RowCount.Should().Be(0);
        }

        [Fact]
        public void SortTable_WithValidColumn_ShouldSucceed()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Score", DataType.Integer)
            };
            _manager.CreateTable("TestTable", columns);

            _manager.AddRow("TestTable", new Dictionary<string, object?> { { "Id", 1 }, { "Score", 50 } });
            _manager.AddRow("TestTable", new Dictionary<string, object?> { { "Id", 2 }, { "Score", 30 } });
            _manager.AddRow("TestTable", new Dictionary<string, object?> { { "Id", 3 }, { "Score", 40 } });

            // Act
            _manager.SortTable("TestTable", "Score", ascending: true);

            // Assert
            var table = _manager.GetTable("TestTable");
            var scores = new List<int>();
            foreach (var row in table.Rows)
            {
                scores.Add(row.GetValue<int>("Score"));
            }
            scores.Should().BeInAscendingOrder();
        }

        [Fact]
        public void GetStatistics_ShouldReturnCorrectData()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");
            var columns = new List<Column> { new Column("Id", DataType.Integer) };
            _manager.CreateTable("Table1", columns);
            _manager.CreateTable("Table2", columns);
            _manager.AddRow("Table1", new Dictionary<string, object?> { { "Id", 1 } });
            _manager.AddRow("Table1", new Dictionary<string, object?> { { "Id", 2 } });

            // Act
            var stats = _manager.GetStatistics();

            // Assert
            stats.DatabaseName.Should().Be("TestDatabase");
            stats.TableCount.Should().Be(2);
            stats.TotalRowCount.Should().Be(2);
        }

        [Fact]
        public void CloseDatabase_ShouldClearCurrentDatabase()
        {
            // Arrange
            _manager.CreateDatabase("TestDatabase");

            // Act
            _manager.CloseDatabase();

            // Assert
            _manager.HasOpenDatabase.Should().BeFalse();
            _manager.CurrentDatabase.Should().BeNull();
            _manager.CurrentFilePath.Should().BeNull();
        }
    }
}
