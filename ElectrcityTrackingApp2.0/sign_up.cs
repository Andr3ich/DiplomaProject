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
            Checkuser();
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

            var login = login_textbox2.Text;
            var password = password_textbox2.Text;

            string querystring = $"insert into Users(user_login, user_password) values('{login}', '{password}')";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

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

        private Boolean Checkuser()
        {
            var loginUser = login_textbox2.Text;
            var passUser = password_textbox2.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();
            string querystring = $"select user_id, user_login, user_password from Users where user_login = '{loginUser}' and user_password = '{passUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Користувач вже існує!");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            log_in frm_login = new log_in();
            frm_login.Show();
            this.Hide();
        }
    }
}
