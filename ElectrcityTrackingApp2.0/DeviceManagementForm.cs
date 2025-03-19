using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Guna.Charts.WinForms;
using Microsoft.Data.SqlClient;
using static ElectrcityTrackingApp2._0.log_in;

namespace ElectrcityTrackingApp2._0
{

    public partial class DeviceManagementForm : Form
    {
        private string connectionString = @"Data Source=BEQUIET;Initial Catalog=ElectricityTracking;Persist Security Info=True;User ID=sa;Password=1q2w3e4r5t;TrustServerCertificate=True";
        private MainForm mainForm;
        private GunaChart pieChart;
        private GunaPieDataset pieDataset;


        public DeviceManagementForm()
        {
            InitializeComponent();
            pieChart = new GunaChart
            {
                Name = "pieСhart",
                Location = new Point(900, 200),
                Size = new Size(800, 800),
                Anchor = AnchorStyles.Right,
                BackColor = Color.FromArgb(49, 51, 56)
            };

            pieChart.YAxes.Display = false;
            pieChart.XAxes.Display = false;
            pieChart.XAxes.GridLines.Display = false;
            pieChart.YAxes.GridLines.Display = false;
            pieChart.Legend.LabelFont = new ChartFont { Name = "Segoe UI", Size = 16 };
            pieChart.Legend.LabelForeColor = Color.FromArgb(128, 255, 128);

            this.Controls.Add(pieChart);
            pieDataset = new GunaPieDataset { Label = "Споживання енергії" };
            pieChart.Datasets.Add(pieDataset);

            LoadCategories();
            LoadDevices();
            LoadPieChart();
            SetupDataGridEvents();
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.AddRange(new string[] {
                "Кухонна техніка",
                "Розваги",
                "Освітлення",
                "Опалення/охолодження",
                "Офісне обладнання",
                "Ванна кімната",
                "Пральня",
                "Інше"
            });
            cmbCategory.SelectedIndex = 0;
        }

        private void SetupDataGridEvents()
        {
            dataGridDevices.CellContentClick += dataGridDevices_CellContentClick;
        }

        private void LoadPieChart()
        {
            if (pieDataset != null)
            {
                pieDataset.DataPoints.Clear();
            }
            else
            {
                MessageBox.Show("Набір даних кругової діаграми не ініціалізовано.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
             SELECT D.name AS [Назва Пристрою], 
                    (Usg.powerconsumption * Usg.averageusagehours) AS [Спожита Енергія] 
             FROM Devices D
             JOIN Usage Usg ON D.device_id = Usg.device_id
             WHERE D.isactive = 1 AND D.user_id = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            double totalEnergy = 0;
                            Dictionary<string, double> energyData = new Dictionary<string, double>();

                            while (reader.Read())
                            {
                                string name = reader["Назва Пристрою"].ToString();
                                double energy = Convert.ToDouble(reader["Спожита Енергія"]);
                                energyData[name] = energy;
                                totalEnergy += energy;
                            }

                            foreach (var item in energyData)
                            {
                                double percentage = (item.Value / totalEnergy) * 100;
                                pieDataset.DataPoints.Add($"{item.Key} ({percentage:F1}%)", (float)percentage);
                            }
                        }
                    }
                }
                pieChart.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження кругової діаграми: " + ex.Message);
            }
        }

        private void LoadDevices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
             SELECT
                 D.device_id AS [ID Пристрою],
                 D.name AS [Назва Пристрою],
                 Usg.powerconsumption AS [Енергоспоживання],
                 Usg.averageusagehours AS [Середній Час Використання],
                 DI.category AS [Категорія],
                 DI.description AS [Опис],
                 DI.dateadded AS [Дата Додавання],
                 D.isactive AS [Активний]
             FROM Users U
             LEFT JOIN Devices D ON U.user_id = D.user_id
             LEFT JOIN DeviceInfo DI ON D.device_id = DI.device_id
             LEFT JOIN Usage Usg ON D.device_id = Usg.device_id
             WHERE U.user_id = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridDevices.DataSource = dataTable;

                        if (dataGridDevices.Columns["Активний"] != null)
                        {
                            DataGridViewCheckBoxColumn activeColumn = new DataGridViewCheckBoxColumn();
                            activeColumn.DataPropertyName = "Активний";
                            activeColumn.Name = "Активний";
                            activeColumn.HeaderText = "Активний";
                            activeColumn.TrueValue = true;
                            activeColumn.FalseValue = false;

                            int columnIndex = dataGridDevices.Columns["Активний"].Index;
                            dataGridDevices.Columns.Remove("Активний");
                            dataGridDevices.Columns.Insert(columnIndex, activeColumn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження пристроїв: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string insertDeviceQuery = @"
                INSERT INTO Devices (name, isactive, user_id)
                VALUES (@Name, 1, @UserId);
                SELECT SCOPE_IDENTITY();"; 

                    int newDeviceId;
                    using (SqlCommand cmd = new SqlCommand(insertDeviceQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        newDeviceId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    string insertUsageQuery = @"
                INSERT INTO Usage (device_id, powerconsumption, averageusagehours)
                VALUES (@DeviceId, @Power, @Hours);";

                    using (SqlCommand cmd = new SqlCommand(insertUsageQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", newDeviceId);
                        cmd.Parameters.AddWithValue("@Power", Convert.ToDouble(txtPower.Text));
                        cmd.Parameters.AddWithValue("@Hours", Convert.ToDouble(txtHours.Text));
                        cmd.ExecuteNonQuery();
                    }

                    string insertDeviceInfoQuery = @"
                INSERT INTO DeviceInfo (device_id, category, description, dateadded)
                VALUES (@DeviceId, @Category, @Description, GETDATE());";

                    using (SqlCommand cmd = new SqlCommand(insertDeviceInfoQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", newDeviceId);
                        cmd.Parameters.AddWithValue("@Category", cmbCategory.Text);
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Пристрій додано успішно!");
                ClearInputs();
                LoadDevices();
                LoadPieChart();
                dataGridDevices.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка додавання пристрою: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridDevices.SelectedRows.Count == 0)
            {
                MessageBox.Show("Виберіть пристрій для видалення.");
                return;
            }

            if (MessageBox.Show("Ви впевнені, що хочете видалити цей пристрій?", "Підтвердити видалення",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    int deviceId = Convert.ToInt32(dataGridDevices.SelectedRows[0].Cells["ID Пристрою"].Value);
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE Devices 
                    SET isactive = 0 
                    WHERE device_id = @DeviceId AND user_id = @UserId", conn))
                        {
                            cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                            cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Пристрій успішно видалено!");
                    ClearInputs();
                    LoadDevices();
                    LoadPieChart();
                    dataGridDevices.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка видалення пристрою: " + ex.Message);
                }
            }
        }


        private void btnBackToMain_Click(object sender, EventArgs e)
        {
            if (mainForm == null || mainForm.IsDisposed)
            {
                mainForm = new MainForm();
                mainForm.FormClosed += MainForm_FormClosed;
            }
            this.Hide();
            mainForm.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            LoadDevices();
            this.Show();
            dataGridDevices.Refresh();
        }


        private void dataGridDevices_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridDevices.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridDevices.SelectedRows[0];
                txtName.Text = row.Cells["Ім'я"].Value.ToString();
                txtPower.Text = row.Cells["Енергоспоживання"].Value.ToString();
                txtHours.Text = row.Cells["AverageUsageHours"].Value.ToString();
                cmbCategory.Text = row.Cells["Категорія"].Value.ToString();
                txtDescription.Text = row.Cells["Опис"].Value.ToString();
            }
        }

        private void ToggleDeviceActivation(int deviceId, bool isActive)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Devices 
                SET isactive = @IsActive 
                WHERE device_id = @DeviceId AND user_id = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                        cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                        cmd.Parameters.AddWithValue("@IsActive", isActive);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadDevices();
                LoadPieChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка зміни статусу пристрою: " + ex.Message);
            }
        }

        private void dataGridDevices_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridDevices.Columns["Активний"].Index && e.RowIndex >= 0)
            {
                int deviceId = Convert.ToInt32(dataGridDevices.Rows[e.RowIndex].Cells["ID Пристрою"].Value);
                bool currentStatus = Convert.ToBoolean(dataGridDevices.Rows[e.RowIndex].Cells["Активний"].Value);

                ToggleDeviceActivation(deviceId, !currentStatus);
            }
        }



        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Будь ласка, введіть назву пристрою.");
                return false;
            }

            if (!double.TryParse(txtPower.Text, out double power) || power <= 0)
            {
                MessageBox.Show("Будь ласка, введіть дійсне значення енергоспоживання.");
                return false;
            }

            if (!double.TryParse(txtHours.Text, out double hours) || hours <= 0 || hours > 24)
            {
                MessageBox.Show("Будь ласка, введіть дійсні години використання (від 0 до 24).");
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtName.Text = "";
            txtPower.Text = "";
            txtHours.Text = "";
            cmbCategory.SelectedIndex = 0;
            txtDescription.Text = "";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = false;
            }
        }

    }
}
