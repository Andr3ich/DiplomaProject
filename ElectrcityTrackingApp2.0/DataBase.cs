using Microsoft.Data.SqlClient;

namespace ElectrcityTrackingApp2._0
{
    internal class DataBase
    {
        private SqlConnection sqlConnection = new SqlConnection(@"Data Source=BEQUIET;Initial Catalog=ElectricityTracking;Persist Security Info=True;User ID=sa;Password=1q2w3e4r5t;TrustServerCertificate=True");

        public void openConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

        }

        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection GetConnection()
        {
            return sqlConnection;
        }
    }
}
