using System;
using System.IO.Ports;

class RedDisplay
{
    static void Main(string[] args)
    {
        string portName = "com7";

        var display = new WeActLCD.Driver();
        display.Connect(portName);
        display.SetOrientation();
        display.SetBrightness(100);
        display.Fill(0, 255, 0);
    }

  
}