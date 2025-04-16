using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace FurnitureProductionSystem
{
    public partial class ProductDialog : Window
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Article { get; set; }
        public decimal MinPrice { get; set; }
        public int ProductTypeID { get; set; }
        public int MaterialID { get; set; }
        public bool IsEditMode { get; set; }

        private readonly string connectionString;

        public ProductDialog(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            this.DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка типов продукции
                using (var connection = new SqlConnection(connectionString))
                {
                    var productTypesAdapter = new SqlDataAdapter(
                        "SELECT TypeID, TypeName FROM ProductTypes", connection);
                    var productTypesTable = new DataTable();
                    productTypesAdapter.Fill(productTypesTable);
                    ProductTypeCombo.ItemsSource = productTypesTable.DefaultView;
                }

                // Загрузка материалов
                using (var connection = new SqlConnection(connectionString))
                {
                    var materialsAdapter = new SqlDataAdapter(
                        "SELECT MaterialID, MaterialName FROM Materials", connection);
                    var materialsTable = new DataTable();
                    materialsAdapter.Fill(materialsTable);
                    MaterialCombo.ItemsSource = materialsTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (ProductTypeCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите тип продукции!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MaterialCombo.SelectedItem == null)
            {
                MessageBox.Show("Выберите материал!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ProductName))
            {
                MessageBox.Show("Введите наименование продукции!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Article))
            {
                MessageBox.Show("Введите артикул!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MinPrice <= 0)
            {
                MessageBox.Show("Введите корректную стоимость!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}