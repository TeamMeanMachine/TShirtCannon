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
            // TODO: Move controller stuff to operator input class
            var controller = new GameController(UsbHostDevice.GetInstance());

            // TODO: move angle motor to a different class
            var angleMotor = new TalonSRX(RobotMap.ANGLE_MOTOR);

            Debug.Print("Program started");


            while(true)
            {
                Debug.Print("Input voltage: " + angleMotor.GetBusVoltage());
                bool isEnabled = controller.GetButton(5);
                if (isEnabled && controller.GetConnectionStatus() == CTRE.Phoenix.UsbDeviceConnection.Connected)
                {
                    CTRE.Phoenix.Watchdog.Feed();
                }

                double turn = controller.GetAxis(2);
                casterDrive.Drive(new Vector2(-controller.GetAxis(0), controller.GetAxis(1)) , -turn);

                // put angle motor in brake mode for now
                angleMotor.SetNeutralMode(NeutralMode.Brake);
            }
        }
    }
}
