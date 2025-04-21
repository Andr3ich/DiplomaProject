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

            string querystring = "SELECT user_id, user_password FROM Users WHERE user_login = @login";

            SqlCommand command = new SqlCommand(querystring, dataBase.GetConnection());
            command.Parameters.Add("@login", SqlDbType.NVarChar).Value = loginUser;

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                string storedHash = table.Rows[0]["user_password"].ToString();

                if (PasswordHasher.VerifyPassword(passUser, storedHash))
                {
                    UserSession.UserId = Convert.ToInt32(table.Rows[0]["user_id"]);
                    MessageBox.Show("Ви успішно увійшли!", "Успішно!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MainForm frm1 = new MainForm();
                    this.Hide();
                    frm1.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Неправильний пароль!", "Помилка входу!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Такого акаунту не існує!", "Акаунту не існує!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            sign_up frm_sign = new sign_up();
            frm_sign.Show();
            this.Hide();
        }
    }
}