using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace FurnitureProductionSystem
{
    public partial class MainWindow : Window
    {
        private string connectionString = @"Data Source=(local);Initial Catalog=FurnitureProductionDB;Integrated Security=True;";

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadProducts();
            LoadWorkshops();
            LoadProductionProcess();
        }

        #region Products Tab Methods

        private void LoadProducts()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                    p.ProductID,
                                    pt.TypeName AS ProductType,
                                    p.Name,
                                    p.Article,
                                    p.MinPartnerPrice,
                                    m.MaterialName AS MainMaterial,
                                    pt.TypeCoefficient,
                                    m.LossPercentage AS MaterialLossPercentage
                                    FROM Products p
                                    JOIN ProductTypes pt ON p.ProductTypeID = pt.TypeID
                                    JOIN Materials m ON p.MaterialID = m.MaterialID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    ProductsGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продукции: {ex.Message}");
            }
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid.SelectedItem is DataRowView row)
            {
                SelectedProductInfo.Text = $"{row["Name"]} (Арт: {row["Article"]})";
            }
            else
            {
                SelectedProductInfo.Text = "Нет";
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog(connectionString);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = @"INSERT INTO Products 
                                        (Name, Article, MinPartnerPrice, ProductTypeID, MaterialID)
                                        VALUES (@Name, @Article, @Price, @TypeID, @MaterialID)";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Name", dialog.ProductName);
                        command.Parameters.AddWithValue("@Article", dialog.Article);
                        command.Parameters.AddWithValue("@Price", dialog.MinPrice);
                        command.Parameters.AddWithValue("@TypeID", dialog.ProductTypeID);
                        command.Parameters.AddWithValue("@MaterialID", dialog.MaterialID);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления продукции: {ex.Message}");
                }
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт для редактирования");
                return;
            }

            DataRowView row = (DataRowView)ProductsGrid.SelectedItem;
            var dialog = new ProductDialog(connectionString)
            {
                ProductID = (int)row["ProductID"],
                ProductName = row["Name"].ToString(),
                Article = row["Article"].ToString(),
                MinPrice = Convert.ToDecimal(row["MinPartnerPrice"]),
                ProductTypeID = GetProductTypeID(row["ProductType"].ToString()),
                MaterialID = GetMaterialID(row["MainMaterial"].ToString()),
                IsEditMode = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        string query = @"UPDATE Products SET 
                                        Name = @Name,
                                        Article = @Article,
                                        MinPartnerPrice = @Price,
                                        ProductTypeID = @TypeID,
                                        MaterialID = @MaterialID
                                        WHERE ProductID = @ProductID";

                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@ProductID", dialog.ProductID);
                        command.Parameters.AddWithValue("@Name", dialog.ProductName);
                        command.Parameters.AddWithValue("@Article", dialog.Article);
                        command.Parameters.AddWithValue("@Price", dialog.MinPrice);
                        command.Parameters.AddWithValue("@TypeID", dialog.ProductTypeID);
                        command.Parameters.AddWithValue("@MaterialID", dialog.MaterialID);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления продукции: {ex.Message}");
                }
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите продукт для удаления");
                return;
            }

            DataRowView row = (DataRowView)ProductsGrid.SelectedItem;
            string productName = row["Name"].ToString();

            if (MessageBox.Show($"Удалить продукт '{productName}'?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        // Сначала удаляем связанные процессы производства
                        string deleteProcessQuery = "DELETE FROM ProductionProcess WHERE ProductID = @ProductID";
                        SqlCommand deleteProcessCommand = new SqlCommand(deleteProcessQuery, connection);
                        deleteProcessCommand.Parameters.AddWithValue("@ProductID", row["ProductID"]);

                        // Затем удаляем сам продукт
                        string deleteProductQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                        SqlCommand deleteProductCommand = new SqlCommand(deleteProductQuery, connection);
                        deleteProductCommand.Parameters.AddWithValue("@ProductID", row["ProductID"]);

                        connection.Open();
                        deleteProcessCommand.ExecuteNonQuery();
                        deleteProductCommand.ExecuteNonQuery();
                    }
                    LoadProducts();
                    LoadProductionProcess();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void RefreshProducts_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void MaterialsReport_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new MaterialsReportWindow(connectionString);
            reportWindow.ShowDialog();
        }

        private int GetProductTypeID(string typeName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT TypeID FROM ProductTypes WHERE TypeName = @TypeName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TypeName", typeName);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        private int GetMaterialID(string materialName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT MaterialID FROM Materials WHERE MaterialName = @MaterialName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MaterialName", materialName);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
        }

        #endregion

        #region Workshops Tab Methods

        private void LoadWorkshops()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                    w.WorkshopID,
                                    w.WorkshopName,
                                    wt.TypeName AS WorkshopType,
                                    w.WorkersRequired
                                    FROM Workshops w
                                    JOIN WorkshopTypes wt ON w.WorkshopTypeID = wt.TypeID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    WorkshopsGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки цехов: {ex.Message}");
            }
        }

        private void RefreshWorkshops_Click(object sender, RoutedEventArgs e)
        {
            LoadWorkshops();
        }

        #endregion

        #region Production Process Tab Methods

        private void LoadProductionProcess()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                    pp.ProcessID,
                                    p.Name AS ProductName,
                                    w.WorkshopName,
                                    pp.ProductionTime
                                    FROM ProductionProcess pp
                                    JOIN Products p ON pp.ProductID = p.ProductID
                                    JOIN Workshops w ON pp.WorkshopID = w.WorkshopID";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    ProductionProcessGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки процессов: {ex.Message}");
            }
        }

        private void RefreshProductionProcess_Click(object sender, RoutedEventArgs e)
        {
            LoadProductionProcess();
        }

        private void CalculateProductionTime_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"SELECT 
                                    p.Name AS ProductName,
                                    SUM(pp.ProductionTime) AS TotalProductionTime
                                    FROM ProductionProcess pp
                                    JOIN Products p ON pp.ProductID = p.ProductID
                                    GROUP BY p.Name";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    string report = "Общее время производства по продуктам:\n\n";
                    foreach (DataRow row in table.Rows)
                    {
                        report += $"{row["ProductName"]}: {row["TotalProductionTime"]} часов\n";
                    }

                    MessageBox.Show(report, "Отчет по времени производства",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета времени: {ex.Message}");
            }
        }

        #endregion
    }
}