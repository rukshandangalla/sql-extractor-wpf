using Dapper;
using EclipseScriptGenerator.models;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EclipseScriptGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<SProc> spList;
        private List<SProc> selectedSPList;

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
            string headerText = File.ReadAllText(@"D:\Projects\RnD\SQL-Checker\sql-extractor-wpf\config\header.sql");
            HEADER_CNT_CTRL.Text = headerText;

            SP_SEARCH_CTRL.IsEnabled = false;
            selectedSPList = new List<SProc>();
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
                    DB_LIST_CTRL.IsEnabled = true;
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

                spList = new List<SProc>();
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT  '[' + ROUTINE_SCHEMA + '].[' + ROUTINE_NAME + ']' AS SPName, ROUTINE_SCHEMA, ROUTINE_NAME, CREATED, LAST_ALTERED FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' ORDER BY LAST_ALTERED DESC;";
                    var result = await connection.QueryAsync<SProc>(query);
                    spList = result.ToList();
                    connection.Close();
                }

                if (spList.Count > 0)
                {
                    SP_LIST_CTRL.ItemsSource = spList;
                    SP_SEARCH_CTRL.IsEnabled = true;
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
        private async void GEN_BTN_CTRL_Click(object sender, RoutedEventArgs e)
        {
            var spList = SP_LIST_CTRL.ItemsSource as List<SProc>;
            var selectedList = new List<SProc>();

            foreach (var sp in spList)
            {
                if (sp.IsSelected)
                {
                    selectedList.Add(sp);
                    await GetTextContent(sp.SPName);
                }
            }
        }

        private async Task GetTextContent(string spName)
        {
            try
            {
                var selectedDB = DB_LIST_CTRL.SelectedItem as DataBase;
                string sqlConnectionString = $"Data Source={SERVER_NAME_CTRL.Text};Initial Catalog={selectedDB.Name};Trusted_Connection=Yes;Max Pool Size=2000;Connection Timeout=300";

                List<string> spLines = new List<string>();
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<string>($"EXEC sp_helptext '{spName}'");
                    spLines = result.ToList();
                    connection.Close();
                }

                string spText = string.Empty;

                // Append Header
                spText += HEADER_CNT_CTRL.Text;

                foreach (var line in spLines)
                {
                    spText += line;
                }

                string fileName = @"D:\Projects\RnD\SQL-Checker\sql-extractor-wpf\SPs\" + spName + ".sql";

                try
                {
                    // Check if file already exists. If yes, delete it.     
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    // Create a new file     
                    using (FileStream fs = File.Create(fileName))
                    {
                        // Add some text to file    
                        byte[] content = new UTF8Encoding(true).GetBytes(spText);
                        fs.Write(content, 0, content.Length);
                    }
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex.ToString());
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Filter loaded SP by name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SP_SEARCH_CTRL_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTxt = SP_SEARCH_CTRL.Text.ToLower();

            if (!string.IsNullOrEmpty(searchTxt))
            {
                var filteredSPs = spList.Where(s => s.SPName.ToLower().Contains(searchTxt));
                SP_LIST_CTRL.ItemsSource = filteredSPs;
            }
            else
            {
                SP_LIST_CTRL.ItemsSource = spList;
            }
        }

        /// <summary>
        /// Add script to export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CHK_BX_CTRL_Checked(object sender, RoutedEventArgs e)
        {
            var currentSP = SP_LIST_CTRL.SelectedItem as SProc;

            if (currentSP.IsSelected)
            {
                SLECTED_SP_LIST_CTRL.Items.Add(SP_LIST_CTRL.SelectedItem as SProc);
            }
        }
    }
}
