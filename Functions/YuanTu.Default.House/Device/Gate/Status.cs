namespace YuanTu.Default.House.Device.Gate
{
    public class Status
    {
        /// <summary>
        /// 闸门开关状态 1：开 0：关
        /// </summary>
        public bool IsOpen { get; set; }
        /// <summary>
        /// 闸门口是否有障碍物状态 1：有 0：无 
        /// </summary>
        public bool HasObstacles { get; set; }
        /// <summary>
        /// 传感器1状态
        /// </summary>
        public byte Sensor1 { get; set; }
        /// <summary>
        /// 传感器2状态
        /// </summary>
        public byte Sensor2 { get; set; }
        /// <summary>
        /// 传感器3状态
        /// </summary>
        public byte Sensor3 { get; set; }
        /// <summary>
        /// 传感器是否坏了 1：坏了 0：没坏
        /// </summary>
        public bool SensorError { get; set; }

        public override string ToString()
        {
            return $"GateStatus:" +
                   $"IsOpen:{IsOpen}" +
                   $"HasObstacles:{HasObstacles}" +
                   $"Sensor1:{Sensor1}" +
                   $"Sensor2:{Sensor2}" +
                   $"Sensor3:{Sensor3}" +
                   $"SensorError:{SensorError}";
        }
    }
}
