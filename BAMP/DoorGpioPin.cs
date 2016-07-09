using System;
using System.Collections.Generic;
using Windows.Devices.Gpio;

namespace BAMP
{
    public class Doors : List<Door>
    {
    }

    public class Door
    {
        public delegate void DoorValueChangedEventArgs(Door door, GpioPinValueChangedEventArgs e);

        public Door(ref GpioController gpio, int pin)
        {
            // open pin
            GpioPin = gpio.OpenPin(pin);

            // Check if input pull-up resistors are supported
            if (GpioPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            { GpioPin.SetDriveMode(GpioPinDriveMode.InputPullUp); }
            else
            { GpioPin.SetDriveMode(GpioPinDriveMode.Input); }

            // Register for the ValueChanged
            GpioPin.ValueChanged += GpioPin_ValueChanged;

            // Set a debounce timeout to filter out switch bounce noise from a button press
            GpioPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
        }

        private void GpioPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            StateChange?.Invoke(this, args);
        }

        public event DoorValueChangedEventArgs StateChange;

        public int Number { get; set; }
        private string _Name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_Name))
                {
                    return "Door_" + Number;
                }
                else
                {
                    return _Name;
                }
            }
            set { _Name = value; }
        }

        public string Description { get; set; }
        public GpioPin GpioPin { get; set; }
        private bool _InUse;

        public bool InUse
        {
            get { return _InUse; }
            set
            {
                if (value)
                {
                    LastClosed = DateTime.Now;
                }
                else
                {
                    LastOpened = DateTime.Now;
                    TotalUsageTime += CurrentElapsedTime;
                }
                _InUse = value;
            }
        }

        public TimeSpan CurrentElapsedTime
        {
            get
            {
                if (InUse)
                { return DateTime.Now - LastClosed; }
                else
                { return DateTime.Now - LastOpened; }
            }
        }

        public TimeSpan TotalUsageTime { get; set; }
        public DateTime LastOpened { get; set; }
        public DateTime LastClosed { get; set; }
    }
}