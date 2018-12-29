using System;
using Microsoft.SPOT;

using CTRE.Phoenix;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace TShirtCannon
{
    class Cannon
    {
        private TalonSRX angleMotor = new TalonSRX(RobotMap.ANGLE_MOTOR);
        private TalonSRX revolverMotor = new TalonSRX(RobotMap.REVOLVER_MOTOR);
        private PneumaticControlModule pcm = new PneumaticControlModule(0);

        private int shootDuration = 88;
        private Stopwatch stopWatch = new Stopwatch();
        private State state = State.READY;
        private bool shootMode;

        private const int INDEXING_TURN_DELAY = 100;
        private const int INDEXING_RETRACT_DELAY = 25;
        private const int INDEXING_LATCH_DELAY = 75;

        private const double REVOLVER_UNITS_TO_DEGREES = 0.2233250620347394;
        private const double REVOLVER_OFFSET = 39.0;
        private const double REVOLVER_HOLDING_ANGLE = 5.0;
        private const double REVOLVER_MIN_ANGLE = 15.0;
        private const double REVOLVER_MAX_ANGLE = 80.0;

        private const double ANGLE_P = 0.015;

        public Cannon()
        {
            angleMotor.SetNeutralMode(NeutralMode.Brake);
            angleMotor.ConfigPeakOutputForward(0.5f);
            angleMotor.ConfigPeakOutputReverse(-0.5f);
            angleMotor.ConfigSelectedFeedbackSensor(FeedbackDevice.Analog);
            angleMotor.ConfigSelectedFeedbackCoefficient(1.0f);
            angleMotor.Config_kP(0.001f);

            revolverMotor.SetInverted(true);
            revolverMotor.SetNeutralMode(NeutralMode.Brake);
            revolverMotor.ConfigPeakOutputForward(0.5f);
            revolverMotor.ConfigPeakOutputReverse(0.5f);

            revolverMotor.ConfigContinuousCurrentLimit(20);
            revolverMotor.ConfigPeakCurrentLimit(0);
            revolverMotor.ConfigPeakCurrentDuration(0);
            revolverMotor.EnableCurrentLimit(true);

            _setpoint = Angle;
        }

        public double Angle
        {
            get { return angleMotor.GetSelectedSensorPosition() *  REVOLVER_UNITS_TO_DEGREES - REVOLVER_OFFSET; }     
         }

        private double _setpoint;

        public double Setpoint
        {
            get { return _setpoint; }
            set
            {
                if (value < REVOLVER_MIN_ANGLE)
                {
                    _setpoint = REVOLVER_MIN_ANGLE;
                }
                else if (value > REVOLVER_MAX_ANGLE)
                {
                    _setpoint = REVOLVER_MAX_ANGLE;
                }
                else
                {
                    _setpoint = value;
                }
                //angleMotor.Set(ControlMode.Position, (_setpoint + REVOLVER_OFFSET) / REVOLVER_UNITS_TO_DEGREES);
            }
        }

        public void DebugPID()
        {
            Debug.Print("Setpoint: " + Setpoint + "\tAngle: " + Angle + "\tOutput: " + angleMotor.GetMotorOutputPercent());
        }

        public void Update(bool shootMode, bool firing, double adjust)
        {
            double setpoint = shootMode ? Setpoint : REVOLVER_HOLDING_ANGLE;
            angleMotor.Set(ControlMode.PercentOutput, (setpoint - Angle) * ANGLE_P);
            if (shootMode)
            {
                Setpoint += adjust;
            }
            else
            {
                _setpoint = 0.0;
            }

            if (state == State.READY)
            {
                if (shootMode && firing)
                {
                    state = State.FIRING;
                    pcm.SetSolenoidOutput(RobotMap.FIRING_SOLENOID, true);
                    stopWatch.Start();
                }
            }
            else if (state == State.FIRING)
            {
                pcm.SetSolenoidOutput(RobotMap.FIRING_SOLENOID, true);
                uint time = stopWatch.DurationMs;
                if (time >= shootDuration)
                {
                    pcm.SetSolenoidOutput(RobotMap.FIRING_SOLENOID, false);
                    state = State.INDEXING;

                    Debug.Print("Shooting time: " + time);
                    stopWatch.Start();
                }
            }
            else if (state == State.INDEXING)
            {
                uint time = stopWatch.DurationMs;
                if (time < INDEXING_TURN_DELAY)
                {
                    pcm.SetSolenoidOutput(RobotMap.INDEXER_SOLENOID, true);
                } 
                else if (time < INDEXING_TURN_DELAY + INDEXING_RETRACT_DELAY)
                {
                    pcm.SetSolenoidOutput(RobotMap.INDEXER_SOLENOID, true);
                    revolverMotor.Set(ControlMode.PercentOutput, 0.4);
                }
                else if (time < INDEXING_TURN_DELAY + INDEXING_RETRACT_DELAY + INDEXING_LATCH_DELAY)
                {
                    pcm.SetSolenoidOutput(RobotMap.INDEXER_SOLENOID, false);
                    revolverMotor.Set(ControlMode.PercentOutput, 0.4);
                }
                else
                {
                    pcm.SetSolenoidOutput(RobotMap.INDEXER_SOLENOID, false);
                    revolverMotor.Set(ControlMode.PercentOutput, 0.0);
                    state = State.READY;
                }
            }
        }

        private enum State {READY, FIRING, INDEXING};
    }
}
