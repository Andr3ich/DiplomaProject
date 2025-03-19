using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace ElectrcityTrackingApp2._0
{
    public partial class log_in : Form
    {
        DataBase dataBase = new DataBase();

        public log_in()
        {
            InitializeComponent();
        }

        public static class UserSession
        {
            public static int UserId { get; set; }
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            var loginUser = login_textbox.Text;
            var passUser = password_textbox.Text;

            SqlDataAdapter adapter = new SqlDataAdapter();
            DataTable table = new DataTable();
            string querystring = $"select user_id, user_login, user_password from Users where user_login = '{loginUser}' and user_password = '{passUser}'";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                UserSession.UserId = Convert.ToInt32(table.Rows[0]["user_id"]);
                MessageBox.Show("Ви успішно увійшли!", "Успішно!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MainForm frm1 = new MainForm();
                this.Hide();
                frm1.ShowDialog();
            }
            else
                MessageBox.Show("Такого акаунту не існує!", "Акаунту не існує!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            sign_up frm_sign = new sign_up();
            frm_sign.Show();
            this.Hide();
        }
    }
}
