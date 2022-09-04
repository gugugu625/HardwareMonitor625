using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HardwareMonitor625
{
    /// <summary>
    /// setting.xaml 的交互逻辑
    /// </summary>
    public partial class setting : Window
    {
        public setting()
        {
            InitializeComponent();
        }

        private void AutoStartup_Checked(object sender, RoutedEventArgs e)
        {
            AutoStart start = new AutoStart("HardwareMonitor625");
            start.SetMeAutoStart((bool)AutoStartup.IsChecked);
        }

        private void restart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Application.Current.Shutdown();
        }
    }
}
