using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace BAMP
{
    public sealed partial class MainPage : Page
    {
        public static readonly int[] DOOR_PINS = { 16 };

        public static Doors Doors = new Doors();

        public WebServer WebServer;

        public MainPage()
        {
            InitializeComponent();
            InitGPIO();
            InitWebServer();
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

                // register for the ValueChanged
                // function is called when the door is opened or closed
                door.StateChange += Door_StateChange;

                // add door to list of doors
                Doors.Add(door);

                door.Number = Doors.IndexOf(door) + 1;
            }
        }

        private void InitWebServer()
        {
            WebServer = new WebServer();
            WebServer.Start();
        }

        private void Door_StateChange(Door door, GpioPinValueChangedEventArgs e)
        {
            // handler gets invoked on a separate thread.
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
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