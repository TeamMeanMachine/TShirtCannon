using System;
using System.Threading;
using Microsoft.SPOT;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.Controller;
using TShirtCannon.Util;


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

            var controllerValues = new GameControllerValues();

            var stopwatch = new Stopwatch();
            var watchdogListener = new Listener(false);
            var shootModeToggler = new Toggler(true);
            var shootListener = new Listener(true);
            var zeroListener = new Listener(false);

            while(true)
            {
                double dt = stopwatch.Duration;
                stopwatch.Start();
                controller.GetAllValues(ref controllerValues);

                bool isEnabled = controller.GetButton(5);
                if (isEnabled && controller.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    Watchdog.Feed();
                }

                double turn = -controller.GetAxis(2);
                casterDrive.Drive(new Vector2(-controller.GetAxis(0), controller.GetAxis(1)) , turn);

                bool shootMode = shootModeToggler.Get(isEnabled && controller.GetButton(1));

                if (zeroListener.Get(controller.GetButton(3))) {
                    Debug.Print("Zeroing Gyro");
                    casterDrive.ZeroGyro();
                }

                bool firing = isEnabled && shootListener.Get(controller.GetButton(6));

                if (watchdogListener.Get(Watchdog.IsEnabled()))
                {
                    cannon.Setpoint = cannon.Angle;
                }

                double adjust;
                if (controllerValues.pov == 0)
                {
                    adjust = 30.0 * dt;
                }
                else if (controllerValues.pov == 4)
                {
                    adjust = -30.0 * dt;
                }
                else
                {
                    adjust = 0.0;
                }

                cannon.Update(shootMode, firing, adjust);

                //Debug.Print(controllerValues.pov +"");
                //Debug.Print(dt +"");
                //cannon.DebugPID();
            }
        }
    }
}
