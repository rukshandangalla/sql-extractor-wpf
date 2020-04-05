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
        }
    }
}
