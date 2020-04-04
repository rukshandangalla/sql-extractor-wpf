using Dapper;
using EclipseScriptGenerator.models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;

namespace EclipseScriptGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        /// <summary>
        /// Initially Load Data
        /// </summary>
        private void LoadData()
        {
            
        }

        /// <summary>
        /// Validate server and make the connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CNNECT_BTN_CTRL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sqlConnectionString = $"Data Source={SERVER_NAME_CTRL.Text};Initial Catalog=Master;Trusted_Connection=Yes;Max Pool Size=2000;Connection Timeout=300";

                List<DataBase> DBs = new List<DataBase>();
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<DataBase>("SELECT database_id, name FROM sys.databases");
                    DBs = result.ToList();
                    connection.Close();
                }

                if(DBs.Count > 0)
                {
                    DB_LIST_CTRL.Visibility = Visibility.Visible;
                    DB_LIST_CTRL.ItemsSource = DBs;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
