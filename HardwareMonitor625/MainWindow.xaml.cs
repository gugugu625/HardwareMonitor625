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
using OpenHardwareMonitor.Hardware;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace HardwareMonitor625
{
    
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer timer;
        Computer myComputer;
        SerialPort DevicePort;
        UpdateVisitor updateVisitor;
        public MainWindow()
        {
            InitializeComponent();

            myComputer = new Computer();
            updateVisitor = new UpdateVisitor();
            myComputer.Open();
            myComputer.CPUEnabled = true;
            myComputer.GPUEnabled = true;
            myComputer.RAMEnabled = true;

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

            timer = new System.Timers.Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(SendData);
            timer.Start();

            DevicePort.Open();
            //Thread.Sleep(100);
            //this.Hide();
            //DevicePort.WriteLine("TEST");
        }
        public HardwareDataStorage GetData(Computer myComputer, UpdateVisitor updateVisitor)
        {
            myComputer.Accept(updateVisitor);
            HardwareDataStorage data = new HardwareDataStorage();
            foreach (var hardwareItem in myComputer.Hardware)
            {
                if (hardwareItem.HardwareType == HardwareType.CPU)
                {
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        //CPU温度
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Name == "CPU Package")
                            {
                                data.CPUTemperature = (float)sensor.Value;
                                //Console.WriteLine(sensor.Name + " " + sensor.Value);
                            }
                        }
                        //CPU占用
                        if (sensor.SensorType == SensorType.Load)
                        {
                            if (sensor.Name == "CPU Total")
                            {
                                data.CPULoad = (float)sensor.Value;
                            }
                        }
                    }
                }
                if (hardwareItem.HardwareType == HardwareType.GpuNvidia)
                {
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        //GPU温度
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Name == "GPU Core")
                            {
                                data.GPUTemperature = (float)sensor.Value;
                            }

                        }
                        //GPU占用
                        if (sensor.SensorType == SensorType.Load)
                        {
                            if (sensor.Name == "GPU Core")
                            {
                                data.GPULoad = (float)sensor.Value;
                            }
                            //GPU内存占用/空闲
                            if (sensor.Name == "GPU Memory")
                            {
                                data.GPUMemoryUsed = (float)sensor.Value;
                            }
                        }
                    }
                }
                if (hardwareItem.HardwareType == HardwareType.RAM)
                {
                    foreach (var sensor in hardwareItem.Sensors)
                    {
                        //内存占用/空闲
                        if (sensor.SensorType == SensorType.Load)
                        {
                            if (sensor.Name == "Memory")
                            {
                                data.RAMUsed = (float)sensor.Value;
                            }
                        }
                    }
                }



            }
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
            HardwareDataStorage storage = GetData(myComputer, updateVisitor);
            CPUTemperature.Content = "CPU温度"+storage.CPUTemperature;
            CPULoad.Content = "CPU占用" + storage.CPULoad;
            GPULoad.Content = "GPU占用" + storage.GPULoad;
            GPUTemperature.Content = "GPU温度" + storage.GPUTemperature;
            GPUMemoryUsed.Content = "GPU内存占用" + storage.GPUMemoryUsed;
            RAMUsed.Content = "内存占用" + storage.RAMUsed;
        }

        private void SendData(object sender, System.Timers.ElapsedEventArgs e)
        {
            HardwareDataStorage storage = GetData(myComputer,updateVisitor);
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
    }
}
