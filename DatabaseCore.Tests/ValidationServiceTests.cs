using DatabaseCore.Models;
using DatabaseCore.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseCore.Tests
{
    public class ValidationServiceTests
    {
        [Theory]
        [InlineData("ValidTableName")]
        [InlineData("Table123")]
        [InlineData("My_Table")]
        [InlineData("ТаблицяУкраїнською")]
        public void ValidateTableName_WithValidName_ShouldReturnSuccess(string tableName)
        {
            // Act
            var result = ValidationService.ValidateTableName(tableName);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123Table")] // Починається з цифри
        [InlineData("Table-Name")] // Містить дефіс
        [InlineData("Table Name")] // Містить пробіл
        public void ValidateTableName_WithInvalidName_ShouldReturnFailure(string tableName)
        {
            // Act
            var result = ValidationService.ValidateTableName(tableName);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().NotBeEmpty();
        }

        [Fact]
        public void ValidateInteger_WithValidValue_ShouldReturnSuccess()
        {
            // Arrange
            var values = new object[] { 123, "456", -789 };

            foreach (var value in values)
            {
                // Act
                var result = ValidationService.ValidateInteger(value);

                // Assert
                result.IsValid.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12.34")]
        [InlineData("not a number")]
        public void ValidateInteger_WithInvalidValue_ShouldReturnFailure(string value)
        {
            // Act
            var result = ValidationService.ValidateInteger(value);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateReal_WithValidValue_ShouldReturnSuccess()
        {
            // Arrange
            var values = new object[] { 123.45, "678.90", 0.001, -5.5 };

            foreach (var value in values)
            {
                // Act
                var result = ValidationService.ValidateReal(value);

                // Assert
                result.IsValid.Should().BeTrue();
            }
        }

        [Fact]
        public void ValidateChar_WithSingleCharacter_ShouldReturnSuccess()
        {
            // Arrange
            var values = new object[] { 'A', "B", 'Я' };

            foreach (var value in values)
            {
                // Act
                var result = ValidationService.ValidateChar(value);

                // Assert
                result.IsValid.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("AB")] // Більше одного символу
        [InlineData("")]   // Порожній рядок
        public void ValidateChar_WithInvalidValue_ShouldReturnFailure(string value)
        {
            // Act
            var result = ValidationService.ValidateChar(value);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateString_WithAnyValue_ShouldReturnSuccess()
        {
            // Arrange
            var values = new object[] { "Hello", "123", "Привіт світ!" };

            foreach (var value in values)
            {
                // Act
                var result = ValidationService.ValidateString(value);

                // Assert
                result.IsValid.Should().BeTrue();
            }
        }

        [Theory]
        [InlineData("100.50")]
        [InlineData("$1,000.00")]
        [InlineData("5000")]
        public void ValidateMoney_WithValidValue_ShouldReturnSuccess(string value)
        {
            // Act
            var result = ValidationService.ValidateMoney(value);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("-100.00")] // Негативне значення
        [InlineData("20000000000000.00")] // Більше максимуму
        [InlineData("invalid")]
        public void ValidateMoney_WithInvalidValue_ShouldReturnFailure(string value)
        {
            // Act
            var result = ValidationService.ValidateMoney(value);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("100.00-500.00")]
        [InlineData("$100.00-$500.00")]
        public void ValidateMoneyInterval_WithValidValue_ShouldReturnSuccess(string value)
        {
            // Act
            var result = ValidationService.ValidateMoneyInterval(value);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("100.00")] // Не інтервал
        [InlineData("500.00-100.00")] // Неправильний порядок
        [InlineData("invalid-range")]
        public void ValidateMoneyInterval_WithInvalidValue_ShouldReturnFailure(string value)
        {
            // Act
            var result = ValidationService.ValidateMoneyInterval(value);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateUniqueColumnNames_WithUniqueNames_ShouldReturnSuccess()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Price", DataType.Money)
            };

            // Act
            var result = ValidationService.ValidateUniqueColumnNames(columns);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateUniqueColumnNames_WithDuplicateNames_ShouldReturnFailure()
        {
            // Arrange
            var columns = new List<Column>
            {
                new Column("Id", DataType.Integer),
                new Column("Name", DataType.String),
                new Column("Id", DataType.String) // Дублікат
            };

            // Act
            var result = ValidationService.ValidateUniqueColumnNames(columns);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ErrorMessage.Should().Contain("дубльовані");
        }

        [Fact]
        public void ValidationResult_ThrowIfInvalid_ShouldThrowWhenInvalid()
        {
            // Arrange
            var result = ValidationResult.Failure("Test error");

            // Act & Assert
            Action act = () => result.ThrowIfInvalid();
            act.Should().Throw<DatabaseCore.Exceptions.ValidationException>()
                .WithMessage("Test error");
        }

        [Fact]
        public void ValidationResult_ThrowIfInvalid_ShouldNotThrowWhenValid()
        {
            // Arrange
            var result = ValidationResult.Success();

            // Act & Assert
            Action act = () => result.ThrowIfInvalid();
            act.Should().NotThrow();
        }
    }
}
