using System;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;

namespace TShirtCannon
{
    class CasterDrive
    {
        public CasterModule[] casters = {
            new CasterModule(RobotMap.LEFT_DRIVE_MOTOR, RobotMap.LEFT_TURN_MOTOR, new Vector2(-14.75, 13.625), -289.672), //left
            new CasterModule(RobotMap.RIGHT_DRIVE_MOTOR, RobotMap.RIGHT_TURN_MOTOR, new Vector2(14.75, 13.625), -77.206), //right
            new CasterModule(RobotMap.BACK_DRIVE_MOTOR, RobotMap.BACK_TURN_MOTOR, new Vector2(0.0, -13.625), -276.680) //back
        };

        private const double TURN_SCALING = 0.7;

        public void Drive (Vector2 translationVelocity, double turn)
        {
            double rotationalVelocity = turn * TURN_SCALING;

            double max = 0.0;
            foreach(var caster in casters)
            {
                double power = caster.ComputePowers(translationVelocity, rotationalVelocity);
                if (power > max)
                {
                    max = power;
                }
            }

            if(max < 1.0)
            {
                max = 1.0;
            }

            foreach(var caster in casters)
            {
                caster.ApplyPowers(max);
            }
        }
    }

    class CasterModule
    {
        private TalonSRX driveTalon;
        private TalonSRX turnTalon;

        private Vector2 center;
        private double angleOffset;
        private Vector2 normalizedRotationalVector;

        private double drivePower;
        private double turnPower;

        private const double ROTATE_SCALE_DOWN_POWER = 1.5;

        public CasterModule(int driveMotorId, int turnMotorId, Vector2 center, double angleOffset)
        {
            driveTalon = new TalonSRX(driveMotorId);
            turnTalon = new TalonSRX(turnMotorId);
            this.center = center;
            this.angleOffset = angleOffset;

            normalizedRotationalVector = new Vector2(center.Y, -center.X).Normalize();

            turnTalon.ConfigSelectedFeedbackSensor(FeedbackDevice.Analog);
            turnTalon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eFeedbackNotContinuous, 1.0f, 0);

            driveTalon.SetNeutralMode(NeutralMode.Brake);
            turnTalon.SetNeutralMode(NeutralMode.Brake);

            driveTalon.ConfigOpenloopRamp(0.1f);
        }

        public double Angle
        {
            get
            {
                int analog;
                turnTalon.GetSensorCollection().GetAnalogInRaw(out analog);
                double voltage = analog * 5.0 / 1024.0;
                return (voltage - 0.2) * 360.0 / 4.6 + angleOffset;
            }
        }

        public double ComputePowers(Vector2 translationVelocity, double rotationalVelocity)
        {
            Vector2 totalVelocity = normalizedRotationalVector * rotationalVelocity + translationVelocity;
            double power = totalVelocity.Length;
            double goalAngle = Units.ToDegrees(Math.Atan2(totalVelocity.X, totalVelocity.Y));

            double angleError = goalAngle - Angle;
            drivePower = power * Math.Cos(Units.ToRadians(angleError));
            turnPower = power * Math.Sin(Units.ToRadians(angleError)) * ROTATE_SCALE_DOWN_POWER;
            return power;
        }

        public void ApplyPowers(double scaleFactor)
        {
            driveTalon.Set(ControlMode.PercentOutput, drivePower / scaleFactor);
            turnTalon.Set(ControlMode.PercentOutput, turnPower / scaleFactor);
        }
    }
}
