using Dapper;
using EclipseScriptGenerator.models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            //Dummy DATA
            //var tempList = new List<SProc>();
            //for (int i = 0; i < 10; i++)
            //{
            //    var sp = new SProc { SPName = $"SPName:{i}", IsSelected = false, CREATED = new DateTime(), LAST_ALTERED = new DateTime() };
            //    tempList.Add(sp);
            //}

            //SP_LIST_CTRL.ItemsSource = tempList;
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

                if (DBs.Count > 0)
                {
                    DB_LIST_CTRL.Visibility = Visibility.Visible;
                    DB_LIST_CTRL.ItemsSource = DBs;
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Load SPs on DB change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DB_LIST_CTRL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedDB = DB_LIST_CTRL.SelectedItem as DataBase;
                string sqlConnectionString = $"Data Source={SERVER_NAME_CTRL.Text};Initial Catalog={selectedDB.Name};Trusted_Connection=Yes;Max Pool Size=2000;Connection Timeout=300";

                List<SProc> spList = new List<SProc>();
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<SProc>("SELECT  '[' + ROUTINE_SCHEMA + '].[' + ROUTINE_NAME + ']' AS SPName, ROUTINE_SCHEMA, ROUTINE_NAME, CREATED, LAST_ALTERED FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE';");
                    spList = result.ToList();
                    connection.Close();
                }

                if (spList.Count > 0)
                {
                    SP_LIST_CTRL.ItemsSource = spList;
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Generate Scripts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GEN_BTN_CTRL_Click(object sender, RoutedEventArgs e)
        {
            var spList = SP_LIST_CTRL.ItemsSource as List<SProc>;
            var selectedList = new List<SProc>();

            foreach (var sp in spList)
            {
                if (sp.IsSelected)
                {
                    selectedList.Add(sp);
                }
            }
        }
    }
}
