using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;
using System.Threading;

namespace HardwareMonitor625
{
    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer timer;
        SerialPort DevicePort;
        public MainWindow()
        {
            InitializeComponent();

            Button exitbutton = new Button { Content = "退出" };
            exitbutton.Click += ExitButton_Click;
            notifyicon.ContextContent = exitbutton;

            COMInit init = new COMInit();
            init.ShowDialog();

            FileStream fileStream = new FileStream("./SerialPort.ini", FileMode.Open);
            StreamReader sr = new StreamReader(fileStream);
            string line;
            if((line = sr.ReadLine()) != null)
            {
                DevicePort = new SerialPort(line.ToString().Trim(), 115200, Parity.None, 8, StopBits.One);
            }

            //GetData();
            timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(SendData);
            timer.Start();

            DevicePort.Open();
            //Thread.Sleep(1000);
            //this.Hide();
            //this.notifyicon.Visibility = Visibility.Visible;
            //DevicePort.WriteLine("TEST");
        }

        public float GetRegistry(string name)
        {
            RegistryKey key = Registry.CurrentUser;
            RegistryKey software = key.OpenSubKey(@"Software\FinalWire\AIDA64\SensorValues", false);
            if (software.GetValue(name) == null)
            {
                return 0;
            }
            string value = software.GetValue(name).ToString();
            key.Close();
            software.Close();
            if (value == "")
            {
                return 0;
            }
            return float.Parse(value);
        }
        public HardwareDataStorage GetData()
        {

            HardwareDataStorage data = new HardwareDataStorage();
            data.CPUTemperature = GetRegistry("Value.TCPU");
            if(data.CPUTemperature == 0)
            {
                data.CPUTemperature = GetRegistry("Value.TCPUPKG");
            }
            data.CPULoad = GetRegistry("Value.SCPUUTI");
            data.GPUTemperature = GetRegistry("Value.TGPU1");
            data.GPULoad = GetRegistry("Value.SGPU1UTI");
            
            float UsedGPUM = GetRegistry("Value.SUSEDVMEM");
            float FreeGPUM = GetRegistry("Value.SFREEVMEM");
            data.GPUMemoryUsed = (float)Math.Round((double)UsedGPUM/(UsedGPUM+FreeGPUM)*100,1);
            data.RAMUsed = GetRegistry("Value.SMEMUTI");
            return data;
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DevicePort.Close();
            Environment.Exit(0);
        }

        private void OnDoubleClick(object sender, RoutedEventArgs e)
        {
            setting newwindow = new setting();
            //newwindow.Show();//打开设置菜单
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HardwareDataStorage storage = GetData();
            CPUTemperature.Content = "CPU温度"+storage.CPUTemperature;
            CPULoad.Content = "CPU占用" + storage.CPULoad;
            GPULoad.Content = "GPU占用" + storage.GPULoad;
            GPUTemperature.Content = "GPU温度" + storage.GPUTemperature;
            GPUMemoryUsed.Content = "GPU内存占用" + storage.GPUMemoryUsed;
            RAMUsed.Content = "内存占用" + storage.RAMUsed;
        }

        private void SendData(object sender, System.Timers.ElapsedEventArgs e)
        {
            HardwareDataStorage storage = GetData();

            /*if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SendData(sender,e));
                return;
            }
            CPUTemperature.Content = "CPU温度" + storage.CPUTemperature;
            CPULoad.Content = "CPU占用" + storage.CPULoad;
            GPULoad.Content = "GPU占用" + storage.GPULoad;
            GPUTemperature.Content = "GPU温度" + storage.GPUTemperature;
            GPUMemoryUsed.Content = "GPU内存占用" + storage.GPUMemoryUsed;
            RAMUsed.Content = "内存占用" + storage.RAMUsed;*/
            Console.WriteLine(JsonConvert.SerializeObject(storage));
            //DevicePort.WriteLine("<<"+Convert.ToBase64String(Encoding.GetEncoding("UTF-8").GetBytes(JsonConvert.SerializeObject(storage)))+">>");
            DevicePort.WriteLine(JsonConvert.SerializeObject(storage));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
