using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// COMInit.xaml 的交互逻辑
    /// </summary>
    public partial class COMInit : Window
    {
        SerialPort DevicePort;
        public COMInit()
        {
            InitializeComponent();
            ThreadStart childref = new ThreadStart(ScanSerialPort);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
        private void ScanSerialPort()
        {
            int ScanCount = 0;
            foreach (string vPortName in SerialPort.GetPortNames())
            {
                DevicePort = new SerialPort(vPortName, 115200, Parity.None, 8, StopBits.One);
                try
                {
                    DevicePort.Open();
                    DevicePort.WriteLine("GetDeviceName");
                    DateTime start = DateTime.Now;
                    while (DevicePort.BytesToRead == 0)
                    {
                        TimeSpan pass = DateTime.Now - start;
                        if (pass.TotalMilliseconds >= 100)
                        {
                            Console.WriteLine("TIMEOUT");
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    Thread.Sleep(50);
                    byte[] recData = new byte[DevicePort.BytesToRead];
                    DevicePort.Read(recData, 0, recData.Length);
                    
                    if (System.Text.Encoding.UTF8.GetString(recData).Trim()== "HardwareMonitor")
                    {
                        ScanCount++;
                        Console.WriteLine(vPortName);
                        File.WriteAllText(@"./SerialPort.ini", vPortName);
                    }

                    //timer.Start();
                    DevicePort.Close();
                }
                catch { }
            }
            if (ScanCount == 0)
            {
                MessageBox.Show("无法检测到设备");
                Environment.Exit(0);
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.DialogResult = true;
            }));
        }
    }
}
