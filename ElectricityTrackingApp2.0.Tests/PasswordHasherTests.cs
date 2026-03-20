using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElectrcityTrackingApp2._0;

namespace ElectrcityTrackingApp2._0.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        // TC-13: HashPassword() генерує хеш, відмінний від вихідного рядка
        [TestMethod]
        public void TC13_HashPassword_ReturnsHashDifferentFromOriginalPassword()
        {
            // Arrange
            string password = "MySecret42";

            // Act
            string hashed = PasswordHasher.HashPassword(password);

            // Assert
            Assert.AreNotEqual(password, hashed);
        }

        // TC-14: VerifyPassword() повертає true для правильного пароля
        [TestMethod]
        public void TC14_VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            string password = "MySecret42";
            string hashed = PasswordHasher.HashPassword(password);

            // Act
            bool result = PasswordHasher.VerifyPassword(password, hashed);

            // Assert
            Assert.IsTrue(result);
        }

        // TC-15: VerifyPassword() повертає false для неправильного пароля
        [TestMethod]
        public void TC15_VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            string password = "MySecret42";
            string hashed = PasswordHasher.HashPassword(password);
            string wrong = "WrongPassword!";

            // Act
            bool result = PasswordHasher.VerifyPassword(wrong, hashed);

            // Assert
            Assert.IsFalse(result);
        }
    }
}