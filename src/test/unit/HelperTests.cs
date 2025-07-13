using Xunit;
using Security;
using System;

namespace unittest
{
    public class HelperTests
    {
        [Fact]
        public void ValidateSecretKey_WithValidKey_DoesNotThrow()
        {
            // Arrange
            var validKey = "valid-secret-key";

            // Act & Assert - Should not throw
            Security.Helpers.ValidateSecretKey(validKey);
        }

        [Fact]
        public void ValidateSecretKey_WithNullKey_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                Security.Helpers.ValidateSecretKey(null));
            
            Assert.Equal("secretKey", exception.ParamName);
        }

        [Fact]
        public void ValidateSecretKey_WithEmptyKey_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                Security.Helpers.ValidateSecretKey(""));
            
            Assert.Equal("secretKey", exception.ParamName);
        }

        [Fact]
        public void ValidateSecretKey_WithWhitespaceKey_DoesNotThrow()
        {
            // Act & Assert - Whitespace is considered valid since method only checks null/empty
            Security.Helpers.ValidateSecretKey("   ");
        }

        [Fact]
        public void ValidateSecretKey_WithTabKey_DoesNotThrow()
        {
            // Act & Assert - Tab is considered valid since method only checks null/empty
            Security.Helpers.ValidateSecretKey("\t");
        }

        [Fact]
        public void ValidateSecretKey_WithNewlineKey_DoesNotThrow()
        {
            // Act & Assert - Newline is considered valid since method only checks null/empty
            Security.Helpers.ValidateSecretKey("\n");
        }

        [Fact]
        public void ValidateSecretKey_WithKeyContainingSpaces_DoesNotThrow()
        {
            // Arrange
            var validKey = "key with spaces";

            // Act & Assert - Should not throw since spaces in the middle are valid
            Security.Helpers.ValidateSecretKey(validKey);
        }

        [Fact]
        public void ValidateSecretKey_WithSpecialCharacters_DoesNotThrow()
        {
            // Arrange
            var validKey = "key-with_special.chars/123";

            // Act & Assert - Should not throw
            Security.Helpers.ValidateSecretKey(validKey);
        }

        [Fact]
        public void ValidateSecretKey_WithVeryLongKey_DoesNotThrow()
        {
            // Arrange - Create a long key (typical AWS parameter names can be up to 1011 characters)
            var longKey = new string('a', 500);

            // Act & Assert - Should not throw
            Security.Helpers.ValidateSecretKey(longKey);
        }

        [Fact]
        public void ValidateSecretKey_WithNumericKey_DoesNotThrow()
        {
            // Arrange
            var numericKey = "12345";

            // Act & Assert - Should not throw
            Security.Helpers.ValidateSecretKey(numericKey);
        }
    }
}