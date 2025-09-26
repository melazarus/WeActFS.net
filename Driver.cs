using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Drawing;

namespace WeActLCD
{
    internal class Driver : IDisposable
    {
        const byte CMD_WHO_AM_I = 0x81;
        const byte CMD_SET_ORIENTATION = 0x02;
        const byte CMD_SET_BRIGHTNESS = 0x03;
        const byte CMD_FULL = 0x04;
        const byte CMD_SET_BITMAP = 0x05;
        const byte CMD_SET_BITMAP_WITH_FASTLZ = 0x15;
        const byte CMD_FREE = 0x07;
        const byte CMD_SYSTEM_VERSION = 0x42;
        const byte CMD_END = 0x0A;
        const byte CMD_READ = 0x80;
        const byte ORIENTATION_PORTRAIT = 0;
        const byte ORIENTATION_LANDSCAPE = 2;
        const byte ORIENTATION_REVERSE_PORTRAIT = 1;
        const byte ORIENTATION_REVERSE_LANDSCAPE = 3;

        private string? _portName;
        private SerialPort? _serialPort;

        public void Connect(string portName)
        {
            _portName = portName;
            if (_portName == null) throw new ArgumentNullException("portName should not be null");
            _serialPort = new(portName, 115200);
            _serialPort.Open();
        }


        public void Fill(int r, int g, int b)
        {
            if (_serialPort == null) Connect(_portName!);

            var color = ConvertToRGB565(r, g, b);

            byte[] command = new byte[12];
            command[0] = 0x04; // CMD_FULL
            command[1] = 0;
            command[2] = 0;
            command[3] = 0;
            command[4] = 0;
            command[5] = 79; // xe - 1 (80-1)
            command[6] = 0;
            command[7] = 159; // ye - 1 (160-1)
            command[8] = 0;
            command[9] = (byte)(color & 0xFF);
            command[10] = (byte)(color >> 8);
            command[11] = 0x0A; // CMD_END

            _serialPort.Write(command, 0, command.Length);
        }

        public void SetOrientation() { 
            byte[] command = [CMD_SET_ORIENTATION, ORIENTATION_PORTRAIT, CMD_END];
            _serialPort.Write(command, 0, command.Length);
        }

        public void SetBrightness(byte level)
        {
            level = (byte)(Math.Clamp((int)level, 0, 100) * 255 / 100);
            ushort brightness_ms = 1000;
            byte[] command =
            [
                CMD_SET_BRIGHTNESS,
                level,
                (byte) (brightness_ms & 0xFF),
                (byte)(brightness_ms >> 8 & 0xFF),
                CMD_END,
            ];
            _serialPort.Write(command, 0, command.Length);
        }
                
        public static ushort ConvertToRGB565(int red, int green, int blue)
        {
            // Clamp values to 0–255
            red = Math.Clamp(red, 0, 255);
            green = Math.Clamp(green, 0, 255);
            blue = Math.Clamp(blue, 0, 255);

            // Convert to RGB565
            ushort r = (ushort)((red >> 3) & 0x1F);      // 5 bits
            ushort g = (ushort)((green >> 2) & 0x3F);    // 6 bits
            ushort b = (ushort)((blue >> 3) & 0x1F);     // 5 bits

            return (ushort)((r << 11) | (g << 5) | b);
        }

        public void Dispose()
        {
            if (_serialPort != null) {
                _serialPort.Close();
                _serialPort.Dispose(); 
            }
        }
    }
}
