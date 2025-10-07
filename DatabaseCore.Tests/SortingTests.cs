using DatabaseCore.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Tests
{
    public class SortingTests
    {
        [Fact]
        public void Sort_ByIntegerColumn_Ascending_ShouldSortCorrectly()
        {
            // Arrange - Створюємо таблицю з цілими числами
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            // Додаємо рядки в невідсортованому порядку
            table.AddRow(new Dictionary<string, object?> { { "Id", 5 }, { "Name", "Five" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 }, { "Name", "Two" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 8 }, { "Name", "Eight" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 1 }, { "Name", "One" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 3 }, { "Name", "Three" } });

            // Act - Сортуємо за колонкою Id (ascending)
            table.Sort("Id", ascending: true);

            // Assert - Перевіряємо правильність сортування
            var ids = table.Rows.Select(r => r.GetValue<int>("Id")).ToList();
            ids.Should().BeInAscendingOrder();
            ids.Should().Equal(1, 2, 3, 5, 8);
        }

        [Fact]
        public void Sort_ByIntegerColumn_Descending_ShouldSortCorrectly()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 5 }, { "Name", "Five" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 }, { "Name", "Two" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 8 }, { "Name", "Eight" } });

            // Act - Сортуємо за Id (descending)
            table.Sort("Id", ascending: false);

            // Assert
            var ids = table.Rows.Select(r => r.GetValue<int>("Id")).ToList();
            ids.Should().BeInDescendingOrder();
            ids.Should().Equal(8, 5, 2);
        }

        [Fact]
        public void Sort_ByStringColumn_ShouldSortAlphabetically()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 1 }, { "Name", "Zebra" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 }, { "Name", "Apple" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 3 }, { "Name", "Mango" } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 4 }, { "Name", "Banana" } });

            // Act
            table.Sort("Name", ascending: true);

            // Assert
            var names = table.Rows.Select(r => r.GetValue<string>("Name")).ToList();
            names.Should().Equal("Apple", "Banana", "Mango", "Zebra");
        }

        [Fact]
        public void Sort_ByRealColumn_ShouldSortCorrectly()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Price", DataType.Real)
            };
            var table = new Table("Products", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 1 }, { "Price", 99.99 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 }, { "Price", 19.50 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 3 }, { "Price", 150.00 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 4 }, { "Price", 5.99 } });

            // Act
            table.Sort("Price", ascending: true);

            // Assert
            var prices = table.Rows.Select(r => r.GetValue<double>("Price")).ToList();
            prices.Should().BeInAscendingOrder();
            prices.First().Should().Be(5.99);
            prices.Last().Should().Be(150.00);
        }

        [Fact]
        public void Sort_ByMoneyColumn_ShouldSortCorrectly()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Salary", DataType.Money)
            };
            var table = new Table("Employees", columns);

            table.AddRow(new Dictionary<string, object?>
            {
                { "Id", 1 },
                { "Salary", new MoneyValue(3500.00m) }
            });
            table.AddRow(new Dictionary<string, object?>
            {
                { "Id", 2 },
                { "Salary", new MoneyValue(5000.00m) }
            });
            table.AddRow(new Dictionary<string, object?>
            {
                { "Id", 3 },
                { "Salary", new MoneyValue(2800.50m) }
            });
            table.AddRow(new Dictionary<string, object?>
            {
                { "Id", 4 },
                { "Salary", new MoneyValue(4200.00m) }
            });

            // Act
            table.Sort("Salary", ascending: true);

            // Assert
            var salaries = table.Rows.Select(r => r.GetValue<MoneyValue>("Salary")!.Amount).ToList();
            salaries.Should().BeInAscendingOrder();
            salaries.Should().Equal(2800.50m, 3500.00m, 4200.00m, 5000.00m);
        }

        [Fact]
        public void Sort_WithNullValues_ShouldPlaceNullsFirst()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Score", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 1 }, { "Score", 100 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 }, { "Score", null } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 3 }, { "Score", 50 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 4 }, { "Score", null } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 5 }, { "Score", 75 } });

            // Act
            table.Sort("Score", ascending: true);

            // Assert
            var scores = table.Rows.Select(r => r.GetValue<int?>("Score")).ToList();

            // Перші два значення мають бути null
            scores[0].Should().BeNull();
            scores[1].Should().BeNull();

            // Решта має бути відсортована
            var nonNullScores = scores.Where(s => s.HasValue).Select(s => s!.Value).ToList();
            nonNullScores.Should().BeInAscendingOrder();
        }

        [Fact]
        public void Sort_ByNonExistentColumn_ShouldThrowException()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 1 }, { "Name", "Test" } });

            // Act & Assert
            Action act = () => table.Sort("NonExistentColumn", ascending: true);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*NonExistentColumn*");
        }

        [Fact]
        public void Sort_EmptyTable_ShouldNotThrowException()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String)
            };
            var table = new Table("TestTable", columns);

            // Act & Assert
            Action act = () => table.Sort("Id", ascending: true);
            act.Should().NotThrow();
            table.RowCount.Should().Be(0);
        }

        [Fact]
        public void Sort_LargeDataset_ShouldSortCorrectly()
        {
            // Arrange - Тестуємо на великій кількості даних
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Value", DataType.Integer)
            };
            var table = new Table("LargeTable", columns);

            var random = new Random(42); // Фіксований seed для відтворюваності
            for (int i = 0; i < 100; i++)
            {
                table.AddRow(new Dictionary<string, object?>
                {
                    { "Id", i },
                    { "Value", random.Next(1, 1000) }
                });
            }

            // Act
            table.Sort("Value", ascending: true);

            // Assert
            var values = table.Rows.Select(r => r.GetValue<int>("Value")).ToList();
            values.Should().BeInAscendingOrder();
            table.RowCount.Should().Be(100);
        }

        [Fact]
        public void Sort_MultipleTimesSameColumn_ShouldWorkCorrectly()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer)
            };
            var table = new Table("TestTable", columns);

            table.AddRow(new Dictionary<string, object?> { { "Id", 3 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 1 } });
            table.AddRow(new Dictionary<string, object?> { { "Id", 2 } });

            // Act - Сортуємо кілька разів
            table.Sort("Id", ascending: true);
            var idsAsc = table.Rows.Select(r => r.GetValue<int>("Id")).ToList();

            table.Sort("Id", ascending: false);
            var idsDesc = table.Rows.Select(r => r.GetValue<int>("Id")).ToList();

            // Assert
            idsAsc.Should().Equal(1, 2, 3);
            idsDesc.Should().Equal(3, 2, 1);
        }
    }
}
