using Microsoft.VisualStudio.TestTools.UnitTesting;
using ElectrcityTrackingApp2._0;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ElectrcityTrackingApp2._0.Tests
{
    [TestClass]
    public class CheckUserTests
    {
        private const string CONNECTION_STRING =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=ElectricityTracking;Integrated Security=True;Trust Server Certificate=True";

        [TestInitialize]
        public void Setup()
        {
            if (!UserExistsInDb("testuser"))
                InsertUser("testuser", PasswordHasher.HashPassword("Test@123"));

            DeleteUser("nobody123");
        }

        //TC-11: CheckUser() повертає true для існуючого логіна
        [TestMethod]
        public void TC11_CheckUser_ExistingLogin_ReturnsTrue()
        {
            // Arrange
            string login = "testuser";

            // Act
            bool result = CheckUserViaDb(login);

            // Assert
            Assert.IsTrue(result);
        }

        // TC-12: CheckUser() повертає false для неіснуючого логіна
        [TestMethod]
        public void TC12_CheckUser_NonExistingLogin_ReturnsFalse()
        {
            // Arrange
            string login = "nobody123";

            // Act
            bool result = CheckUserViaDb(login);

            // Assert
            Assert.IsFalse(result);
        }

        private bool CheckUserViaDb(string loginUser)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            var cmd = new SqlCommand(
                "SELECT user_id FROM Users WHERE user_login = @login", conn);
            cmd.Parameters.Add("@login", SqlDbType.NVarChar).Value = loginUser;

            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table.Rows.Count > 0;
        }

        private void InsertUser(string login, string hashedPassword)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            conn.Open();
            var cmd = new SqlCommand(
                "INSERT INTO Users(user_login, user_password) VALUES(@login, @password)", conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", hashedPassword);
            cmd.ExecuteNonQuery();
        }

        private void DeleteUser(string login)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            conn.Open();
            var cmd = new SqlCommand(
                "DELETE FROM Users WHERE user_login = @login", conn);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.ExecuteNonQuery();
        }

        private bool UserExistsInDb(string login)
        {
            using var conn = new SqlConnection(CONNECTION_STRING);
            var cmd = new SqlCommand(
                "SELECT user_id FROM Users WHERE user_login = @login", conn);
            cmd.Parameters.AddWithValue("@login", login);
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table.Rows.Count > 0;
        }
    }
}