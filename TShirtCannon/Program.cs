using System;
using System.Threading;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Controller;


namespace TShirtCannon
{
    public class Program
    {
        public static void Main()
        {
            var casterDrive = new CasterDrive();

            // uncomment this if you need to zero the swerves
            /*while (true)
            {
                Debug.Print("Angle: " + casterDrive.casters[0].Angle + "\t" + casterDrive.casters[1].Angle + "\t" + casterDrive.casters[2].Angle);
            }*/

            var cannon = new Cannon();
            // TODO: Move controller stuff to operator input class
            var controller = new GameController(UsbHostDevice.GetInstance());

            Debug.Print("Program started");

            while(true)
            {
                bool isEnabled = controller.GetButton(5);
                if (isEnabled && controller.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    Watchdog.Feed();
                }

                double turn = -controller.GetAxis(2);
                casterDrive.Drive(new Vector2(-controller.GetAxis(0), controller.GetAxis(1)) , turn);

                bool firing = isEnabled && controller.GetButton(6);
                cannon.Update(firing);
            }
        }
    }
}
