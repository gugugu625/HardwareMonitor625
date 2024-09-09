using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Xml.Linq;

namespace HardwareMonitor625
{
    /// <summary>
    /// COMInit.xaml 的交互逻辑
    /// </summary>
    public partial class COMInit : Window
    {
        SerialPort DevicePort;
        string logPath = "./com_logs.txt";
        string saveFile = "./SerialPort.ini";
        public COMInit()
        {
            InitializeComponent();
            ThreadStart childref = new ThreadStart(ScanSerialPort);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
        private bool CheckPort(string vPortName, StreamWriter logger)
        {
            bool res = false;
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
                        logger.WriteLine(vPortName + ":" + "TIMEOUT");
                        break;
                    }
                    Thread.Sleep(1);
                }
                Thread.Sleep(50);
                byte[] recData = new byte[DevicePort.BytesToRead];
                DevicePort.Read(recData, 0, recData.Length);

                if (System.Text.Encoding.UTF8.GetString(recData).Trim() == "HardwareMonitor")
                {
                        
                    Console.WriteLine(vPortName);
                    logger.WriteLine(vPortName + ":" + "Find port");
                    res = true;
                }
                else
                {
                    logger.WriteLine(vPortName + ":" + "wrong response. res:" + Encoding.UTF8.GetString(recData).Trim());
                }


                //timer.Start();
                DevicePort.Close();
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
        private void ScanSerialPort()
        {
            int ScanCount = 0;
            using (StreamWriter logger = new StreamWriter(logPath, false))
            {
                logger.WriteLine("Start check");


                if (File.Exists(saveFile))
                {
                    if (!IsFileEmpty(saveFile))
                    {
                        string port = File.ReadAllText(saveFile);
                        if (CheckPort(port, logger))
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.DialogResult = true;
                            }));
                            return;
                        };
                    }
                }
                logger.WriteLine("fallback scan");
                
                foreach (string vPortName in SerialPort.GetPortNames())
                {
                    if (CheckPort(vPortName, logger))
                    {
                        File.WriteAllText(saveFile, vPortName);
                        ScanCount++;
                    };

                }
                logger.WriteLine("success port:"+ScanCount.ToString());
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
        private bool IsFileEmpty(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            return fileInfo.Length == 0;
        }
    }
}
