using System;
using CTRE.Phoenix.Sensors;
using CTRE.Phoenix.MotorControl.CAN;
using CTRE.Phoenix.MotorControl;

namespace TShirtCannon
{
    class CasterDrive
    {
        public CasterModule[] casters;
        private PigeonIMU gyro;
        private float[] gyroAngles = new float[3];

        private const double TURN_SCALING = 0.7;

        public CasterDrive()
        {
            var leftDriveTalon = new TalonSRX(RobotMap.LEFT_DRIVE_MOTOR);
            gyro = new PigeonIMU(leftDriveTalon);

            casters = new CasterModule[] {
                new CasterModule(leftDriveTalon, new TalonSRX(RobotMap.LEFT_TURN_MOTOR), new Vector2(-14.75, 13.625), 0),  //left
                new CasterModule(new TalonSRX(RobotMap.RIGHT_DRIVE_MOTOR), new TalonSRX(RobotMap.RIGHT_TURN_MOTOR), new Vector2(14.75, 13.625), 0), //right
                new CasterModule(new TalonSRX(RobotMap.BACK_DRIVE_MOTOR), new TalonSRX(RobotMap.BACK_TURN_MOTOR), new Vector2(0.0, -13.625), 0), //back
            };
        }

        public void Drive (Vector2 translationVelocity, double turn)
        {
            double rotationalVelocity = turn * TURN_SCALING;

            gyro.GetYawPitchRoll(gyroAngles);
            double gyroAngle = gyroAngles[0];

            double max = 0.0;
            foreach(var caster in casters)
            {
                double power = caster.ComputePowers(translationVelocity, rotationalVelocity, gyroAngle);
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

        public void ZeroGyro()
        {
            gyro.SetYaw(0.0f);
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

        public CasterModule(TalonSRX driveTalon, TalonSRX turnTalon, Vector2 center, double angleOffset)
        {
            this.driveTalon = driveTalon;
            this.turnTalon = turnTalon;
            this.center = center;
            this.angleOffset = angleOffset;

            normalizedRotationalVector = new Vector2(center.Y, -center.X).Normalize();

            this.turnTalon.ConfigSelectedFeedbackSensor(FeedbackDevice.Analog);
            this.turnTalon.ConfigSetParameter(CTRE.Phoenix.LowLevel.ParamEnum.eFeedbackNotContinuous, 1.0f, 0);
            this.turnTalon.ConfigContinuousCurrentLimit(15);
            this.turnTalon.ConfigPeakCurrentLimit(0);
            this.turnTalon.ConfigPeakCurrentDuration(0);
            this.turnTalon.EnableCurrentLimit(true);

            this.driveTalon.SetNeutralMode(NeutralMode.Brake);
            this.turnTalon.SetNeutralMode(NeutralMode.Brake);

            this.driveTalon.ConfigOpenloopRamp(0.1f);
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

        public double ComputePowers(Vector2 translationVelocity, double rotationalVelocity, double gyroAngle)
        {
            // adjust translationVelocity by gyroAngle before adding the rotation to it
            //double translationSize = translationVelocity.Length;
            //double translationAngle = Units.ToDegrees(Math.Atan2(translationVelocity.X, translationVelocity.Y)) - gyroAngle;
            //translationVelocity.X = -Math.Sin(Units.ToRadians(translationAngle)) * translationSize;
            //translationVelocity.Y = Math.Cos(Units.ToRadians(translationAngle)) * translationSize;

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
