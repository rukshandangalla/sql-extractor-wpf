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
        private int scriptCounter = 0;

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
            string headerText = File.ReadAllText(@"header.sql");
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
            foreach (var sp in selectedSPList)
            {
                await GetTextContent(sp);
            }
        }

        /// <summary>
        /// Get text for given SP
        /// </summary>
        /// <param name="spName"></param>
        /// <returns></returns>
        private async Task GetTextContent(SProc sp)
        {
            try
            {
                
                var selectedDB = DB_LIST_CTRL.SelectedItem as DataBase;
                string sqlConnectionString = $"Data Source={SERVER_NAME_CTRL.Text};Initial Catalog={selectedDB.Name};Trusted_Connection=Yes;Max Pool Size=2000;Connection Timeout=300";

                List<string> spLines = new List<string>();
                using (var connection = new SqlConnection(sqlConnectionString))
                {
                    await connection.OpenAsync();
                    var result = await connection.QueryAsync<string>($"EXEC sp_helptext '{sp.SPName}'");
                    spLines = result.ToList();
                    connection.Close();
                }

                string spText = string.Empty;

                //Create name of the file
                var scriptNumber = (Convert.ToInt32(ESCRIPT_NUMBER_CTRL.Text) + scriptCounter).ToString("000");
                var fileName = $"{scriptNumber}_EScript_{sp.ROUTINE_NAME}_{UID_NAME_CTRL.Text}_{AUTHOR_NAME_CTRL.Text}.sql";
                string path = $"D:\\Projects\\RnD\\SQL-Checker\\sql-extractor-wpf\\SPs\\{fileName}";

                //Replace values in content
                var headerContent = HEADER_CNT_CTRL.Text;
                headerContent = headerContent.Replace("##RELEASE_NUMBER##", RELEASE_NUMBER_CTRL.Text);
                headerContent = headerContent.Replace("##SCRIPT_NUMBER##", scriptNumber);
                headerContent = headerContent.Replace("##AUTHOR_NAME##", AUTHOR_NAME_CTRL.Text);
                headerContent = headerContent.Replace("##STORY_NAME##", UID_NAME_CTRL.Text);
                headerContent = headerContent.Replace("##SP_NAME##", sp.SPName);

                //Append Header
                spText += headerContent;

                foreach (var line in spLines)
                {
                    spText += line;
                }

                // Check if file already exists. If yes, delete it.     
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Create a new file     
                using (FileStream fs = File.Create(path))
                {
                    // Add some text to file    
                    byte[] content = new UTF8Encoding(true).GetBytes(spText);
                    fs.Write(content, 0, content.Length);
                }

                scriptCounter++;
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
        /// Add/Remove script to export
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CHK_BX_CTRL_Checked(object sender, RoutedEventArgs e)
        {
            var currentSP = SP_LIST_CTRL.SelectedItem as SProc;

            if (currentSP.IsSelected)
            {
                selectedSPList.Add(currentSP);
                SLECTED_SP_LIST_CTRL.Items.Add(currentSP);
            }
            else
            {
                selectedSPList.Remove(currentSP);
                SLECTED_SP_LIST_CTRL.Items.Remove(currentSP);
            }
        }

        /// <summary>
        /// Clear all selection at once
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CLEAR_EXPORTS_BTN_CTRL_Click(object sender, RoutedEventArgs e)
        {
            selectedSPList.Clear();
            SLECTED_SP_LIST_CTRL.Items.Clear();

            //Uncheck from main grid
            foreach (var sp in spList)
            {
                sp.IsSelected = false;
            }

            SP_LIST_CTRL.ItemsSource = null;
            SP_LIST_CTRL.ItemsSource = spList;
        }
    }
}
