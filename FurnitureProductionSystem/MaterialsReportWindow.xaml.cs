using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace FurnitureProductionSystem
{
    public partial class MaterialsReportWindow : Window
    {
        public MaterialsReportWindow(string connectionString)
        {
            InitializeComponent();
            LoadMaterialsReport(connectionString);
        }

        private void LoadMaterialsReport(string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Исправленный запрос с правильными именами столбцов
                    string query = @"SELECT 
                                    m.MaterialName AS MaterialType,
                                    COUNT(p.MaterialID) AS ProductsCount,
                                    AVG(m.LossPercentage) AS AverageLoss
                                    FROM Materials m
                                    LEFT JOIN Products p ON m.MaterialID = p.MaterialID
                                    GROUP BY m.MaterialName";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    MaterialsReportGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отчета по материалам: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

