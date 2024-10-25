using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace FinanceApp
{
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=FinanceDB;Uid=root;Pwd=12345678;";

        public MainWindow()
        {
            InitializeComponent();
            UpdateBalance();
            LoadRecords();
        }

        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            AddRecord("income", Convert.ToDecimal(AmountTextBox.Text));
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            AddRecord("expense", Convert.ToDecimal(AmountTextBox.Text));
        }

        private void AddRecord(string type, decimal amount)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO FinancialRecords (Type, Amount, Date) VALUES (@Type, @Amount, @Date)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Type", type);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@Date", DateTime.Now);
                    command.ExecuteNonQuery();
                }
            }
            UpdateBalance();
            LoadRecords();  // Обновляем таблицу после добавления записи
        }

        private void UpdateBalance()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SUM(CASE WHEN Type = 'income' THEN Amount ELSE -Amount END) AS Balance FROM FinancialRecords";
                using (var command = new MySqlCommand(query, connection))
                {
                    var balance = command.ExecuteScalar();
                    BalanceTextBlock.Text = balance != DBNull.Value ? balance.ToString() : "0";
                }
            }
        }

        private void LoadRecords()
        {
            List<FinancialRecord> records = new List<FinancialRecord>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Type, Amount, Date FROM FinancialRecords";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new FinancialRecord
                            {
                                Type = reader["Type"].ToString(),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                Date = Convert.ToDateTime(reader["Date"])
                            });
                        }
                    }
                }
            }

            RecordsDataGrid.ItemsSource = records;  // Привязываем данные к DataGrid
        }

        private void ClearTable_Click(object sender, RoutedEventArgs e)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM FinancialRecords";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            UpdateBalance();
            LoadRecords();  // Обновляем таблицу после очистки
        }
    }
}
