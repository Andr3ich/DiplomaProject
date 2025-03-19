using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using static ElectrcityTrackingApp2._0.log_in;

namespace ElectrcityTrackingApp2._0
{
    public partial class MainForm : Form
    {
        private string connectionString = @"Data Source=BEQUIET;Initial Catalog=ElectricityTracking;Persist Security Info=True;User ID=sa;Password=1q2w3e4r5t;TrustServerCertificate=True";
        private DeviceManagementForm deviceForm;

        public MainForm()
        {
            InitializeComponent();
            InitializeChart();
            LoadDevices();
            InitializeComboBox();
        }

        private void LoadDevices()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT name FROM Devices WHERE isactive = 1 AND user_id = @UserId", conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserSession.UserId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        cmbDevices.Items.Clear();
                        cmbDevices.Items.Add("Заповнити самостійно");
                        foreach (DataRow row in dataTable.Rows)
                        {
                            cmbDevices.Items.Add(row["name"].ToString());
                        }
                        cmbDevices.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при завантаженні пристроїв: " + ex.Message);
            }
        }

        private void InitializeComboBox()
        {
            cmbPeriod.Items.Clear();
            cmbPeriod.Items.Add("1 День");
            cmbPeriod.Items.Add("5 Днів");
            cmbPeriod.Items.Add("1 Тиждень");
            cmbPeriod.Items.Add("1 Місяць");
            cmbPeriod.Items.Add("1 Рік");
            cmbPeriod.SelectedIndex = 0;
        }

       private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevices.SelectedIndex == 0)
            {
                txtUsageTime.Text = "";
                txtPowerConsumption.Text = "";
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT u.powerconsumption, u.averageusagehours, d.category, d.description, d.dateadded " +
                        "FROM Devices dv " +
                        "JOIN Usage u ON dv.device_id = u.device_id " +
                        "JOIN DeviceInfo d ON dv.device_id = d.device_id " +
                        "WHERE dv.name = @Name AND dv.isactive = 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", cmbDevices.SelectedItem.ToString());
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtPowerConsumption.Text = reader["powerconsumption"].ToString();
                                txtUsageTime.Text = reader["averageusagehours"].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка завантаження даних про пристрій: " + ex.Message);
            }
       }
    

        private void btnOpenDeviceManager_Click(object sender, EventArgs e)
        {
            if (deviceForm == null || deviceForm.IsDisposed)
            {
                deviceForm = new DeviceManagementForm();
                deviceForm.FormClosed += DeviceForm_FormClosed;
            }
            this.Hide();
            deviceForm.Show();
        }

        private void DeviceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            LoadDevices();
            this.Show();
        }

        private void InitializeChart()
        {
            chart1.Title.Text = "Споживання електроенергії";
            chart1.Title.Font = new Guna.Charts.WinForms.ChartFont()
            {
                FontName = "Segoe UI",
                Size = 24,
                Style = Guna.Charts.WinForms.ChartFontStyle.Bold
            };

            chart1.Legend.LabelFont = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };

            chart1.XAxes.GridLines.Display = true;
            chart1.YAxes.GridLines.Display = true;
            chart1.XAxes.Ticks.Font = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };
            chart1.YAxes.Ticks.Font = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };

            Guna.Charts.WinForms.GunaLineDataset currentUsage = new Guna.Charts.WinForms.GunaLineDataset()
            {
                Label = "Поточне споживання",
                FillColor = Color.FromArgb(35, 165, 90),
                BorderColor = Color.FromArgb(35, 165, 90),
                PointRadius = 3
                
            };

            Guna.Charts.WinForms.GunaLineDataset maxLimit = new Guna.Charts.WinForms.GunaLineDataset()
            {
                Label = "Встановлений ліміт",
                FillColor = Color.FromArgb(211, 55, 58),
                BorderColor = Color.FromArgb(211, 55, 58),
                PointRadius = 0
            };

            chart1.Datasets.Clear();
            chart1.Datasets.Add(currentUsage);
            chart1.Datasets.Add(maxLimit);

            chart1.Update();

            chart2.Title.Text = "Витрати на електроенергію";
            chart2.Title.Font = new Guna.Charts.WinForms.ChartFont()
            {
                FontName = "Segoe UI",
                Size = 24,
                Style = Guna.Charts.WinForms.ChartFontStyle.Bold
            };

            chart2.Legend.LabelFont = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };

            chart2.XAxes.GridLines.Display = true;
            chart2.YAxes.GridLines.Display = true;
            chart2.XAxes.Ticks.Font = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };
            chart2.YAxes.Ticks.Font = new Guna.Charts.WinForms.ChartFont() { FontName = "Segoe UI" };

            Guna.Charts.WinForms.GunaBarDataset currentExpenses = new Guna.Charts.WinForms.GunaBarDataset()
            {
                Label = "Поточні витрати",
                FillColors = { Color.FromArgb(35, 165, 90) },
            };

            Guna.Charts.WinForms.GunaBarDataset maxExpensesLimit = new Guna.Charts.WinForms.GunaBarDataset()
            {
                Label = "Ліміт витрат",
                FillColors = { Color.FromArgb(211, 55, 58) },
            };

            Guna.Charts.WinForms.ChartFont legendFont = new Guna.Charts.WinForms.ChartFont()
            {
                FontName = "Segoe UI",
                Size = 16,
            };

            chart1.Legend.LabelFont = legendFont;
            chart2.Legend.LabelFont = legendFont;

            Guna.Charts.WinForms.ChartFont axisFont = new Guna.Charts.WinForms.ChartFont()
            {
                FontName = "Segoe UI",
                Size = 14, 
            };

            chart1.XAxes.Ticks.Font = axisFont;
            chart1.YAxes.Ticks.Font = axisFont;

            chart2.XAxes.Ticks.Font = axisFont;
            chart2.YAxes.Ticks.Font = axisFont;


            chart2.Datasets.Clear();
            chart2.Datasets.Add(currentExpenses);
            chart2.Datasets.Add(maxExpensesLimit);

            chart2.Update();
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    MessageBox.Show(
                        "Будь ласка, введіть дійсні числові значення у всіх полях.",
                        "Помилка вводу даних",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                double usageTime = Convert.ToDouble(txtUsageTime.Text);
                double powerConsumption = Convert.ToDouble(txtPowerConsumption.Text);
                double pricePerKWh = Convert.ToDouble(txtPricePerKWh.Text);
                double maxLimit = Convert.ToDouble(txtMaxLimit.Text);

                int days = GetDaysFromPeriod();

                double dailyConsumption = (usageTime * powerConsumption) / 1000;
                double totalConsumption = dailyConsumption * days;
                double totalCost = totalConsumption * pricePerKWh;
                double moneyLimit = Convert.ToDouble(txtMoneyLimit.Text);
                UpdateExpensesChart(totalCost, moneyLimit);



                lblTotalConsumption.Text = $"Загальне споживання: {totalConsumption:F2} кВТ/год";
                lblTotalCost.Text = $"Загальна ціна: {totalCost:F2} грн";

                UpdateChart(totalConsumption, maxLimit, days);

                if (totalConsumption > maxLimit)
                {
                    MessageBox.Show(
                        "Попередження: Ваше споживання електроенергії перевищує встановлений ліміт!",
                        "Попередження про споживання",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
                if (totalCost > moneyLimit)
                {
                    MessageBox.Show(
                        "Попередження: Ваші витрати на електроенергію перевищують встановлений фінансовий ліміт!",
                        "Попередження про витрати",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Сталася помилка: {ex.Message}",
                    "Помилка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private bool ValidateInputs()
        {
            return double.TryParse(txtUsageTime.Text, out double usageTime) && usageTime > 0 &&
                   double.TryParse(txtPowerConsumption.Text, out double powerConsumption) && powerConsumption > 0 &&
                   double.TryParse(txtPricePerKWh.Text, out double pricePerKWh) && pricePerKWh > 0 &&
                   double.TryParse(txtMaxLimit.Text, out double maxLimit) && maxLimit > 0;
        }

        private int GetDaysFromPeriod()
        {
            if (cmbPeriod.SelectedItem == null) return 1;

            switch (cmbPeriod.SelectedItem.ToString())
            {
                case "1 День": return 1;
                case "5 Днів": return 5;
                case "1 Тиждень": return 7;
                case "1 Місяць": return 30;
                case "1 Рік": return 365;
                default: return 1;
            }
        }

        private void UpdateChart(double totalConsumption, double maxLimit, int days)
        {
            var currentUsageDataset = chart1.Datasets.OfType<Guna.Charts.WinForms.GunaLineDataset>()
                .FirstOrDefault(ds => ds.Label == "Поточне споживання");
            var maxLimitDataset = chart1.Datasets.OfType<Guna.Charts.WinForms.GunaLineDataset>()
                .FirstOrDefault(ds => ds.Label == "Встановлений ліміт");

            if (currentUsageDataset == null || maxLimitDataset == null)
                return;

            currentUsageDataset.DataPoints.Clear();
            maxLimitDataset.DataPoints.Clear();

            for (int i = 0; i <= days; i++)
            {
                double currentValue = (totalConsumption / days) * i;
                currentUsageDataset.DataPoints.Add(i.ToString(), currentValue);
                maxLimitDataset.DataPoints.Add(i.ToString(), maxLimit);
            }
            chart1.Update();
        }

        private void UpdateExpensesChart(double totalCost, double moneyLimit)
        {
            var currentExpensesDataset = chart2.Datasets.OfType<Guna.Charts.WinForms.GunaBarDataset>()
                .FirstOrDefault(ds => ds.Label == "Поточні витрати");
            var maxExpensesDataset = chart2.Datasets.OfType<Guna.Charts.WinForms.GunaBarDataset>()
                .FirstOrDefault(ds => ds.Label == "Ліміт витрат");

            if (currentExpensesDataset == null || maxExpensesDataset == null)
                return;

            currentExpensesDataset.DataPoints.Clear();
            maxExpensesDataset.DataPoints.Clear();

            currentExpensesDataset.DataPoints.Add("Витрати", totalCost);
            maxExpensesDataset.DataPoints.Add("Ліміт", moneyLimit);

            chart2.Update();
        }
    }
}