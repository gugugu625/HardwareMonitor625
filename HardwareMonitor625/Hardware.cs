using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;

namespace HardwareMonitor625
{
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware)
                subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }

    public class HardwareDataStorage
    {
        public float CPUTemperature { get; set; }
        public float CPULoad { get; set; }
        public float GPUTemperature { get; set; }
        public float GPULoad { get; set; }
        public float GPUMemoryUsed { get; set; }
        public float RAMUsed { get; set; }
        public HardwareDataStorage()
        {
            CPUTemperature = 0;
            CPULoad = 0;
            GPUTemperature = 0;
            GPULoad = 0;
            GPUMemoryUsed = 0;
            RAMUsed = 0;
        }
    }
}
