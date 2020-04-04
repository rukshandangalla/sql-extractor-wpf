using System.Windows;
using System.Windows.Media;

namespace EclipseScriptGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManagerHelper.CreateTheme("Dark", Colors.Red);

            
            //thisConnection.Open();

            //string Get_Data = "SELECT database_id, name FROM sys.databases";

            //SqlCommand cmd = thisConnection.CreateCommand();
            //cmd.CommandText = Get_Data;

            //SqlDataAdapter sda = new SqlDataAdapter(cmd);
            //DataTable dt = new DataTable("emp");
            //sda.Fill(dt);
        }
    }
}
