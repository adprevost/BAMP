using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using static BAMP.Door;

namespace BAMP
{
    public sealed partial class MainPage : Page
    {

        public static readonly int[] DOOR_PINS = { 16 };

        private Doors doors = new Doors();

        public MainPage()
        {
            InitializeComponent();
            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            foreach (var pin in DOOR_PINS)
            {
                // create new door from pin
                Door door = new Door(ref gpio, pin);

                // Register for the ValueChanged event so our buttonPin_ValueChanged 
                // function is called when the button is pressed
                door.StateChange += Door_StateChange;

                // add door to list of doors
                doors.Add(door);

                door.Number = doors.IndexOf(door) + 1;
            }
        }

        private void Door_StateChange(Door door, GpioPinValueChangedEventArgs e)
        {
            // need to invoke UI updates on the UI thread because this event
            // handler gets invoked on a separate thread.
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    // door closed
                    door.InUse = true;
                    Debug.WriteLine("InUse");
                }
                else
                {
                    // door open
                    door.InUse = false;
                    Debug.WriteLine("NotInUse");
                }
            });
        }
    }
}