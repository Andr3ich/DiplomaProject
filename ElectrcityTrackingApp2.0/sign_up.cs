using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace ElectrcityTrackingApp2._0
{
    public partial class sign_up : Form
    {
        DataBase dataBase = new DataBase();

        public sign_up()
        {
            InitializeComponent();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            var login = login_textbox2.Text;
            var password = password_textbox2.Text;

            if (CheckUser(login))
            {
                MessageBox.Show("Користувач вже існує!");
                return;
            }

            string hashedPassword = PasswordHasher.HashPassword(password);

            string querystring = $"INSERT INTO Users(user_login, user_password) VALUES(@login, @password)";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());
            command.Parameters.Add("@login", SqlDbType.NVarChar).Value = login;
            command.Parameters.Add("@password", SqlDbType.NVarChar).Value = hashedPassword;

            dataBase.openConnection();

            if (command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Акаунт створено!", "Успіх!");
                log_in frm_login = new log_in();
                this.Hide();
                frm_login.ShowDialog();
            }
            else
            {
                MessageBox.Show("Акаунт не створено!");
            }

            dataBase.closeConnection();
        }

        private Boolean CheckUser(string loginUser)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();

            string querystring = "SELECT user_id FROM Users WHERE user_login = @login";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());
            command.Parameters.Add("@login", SqlDbType.NVarChar).Value = loginUser;

            adapter.SelectCommand = command;
            adapter.Fill(table);

            return table.Rows.Count > 0;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            log_in frm_login = new log_in();
            frm_login.Show();
            this.Hide();
        }
    }
}