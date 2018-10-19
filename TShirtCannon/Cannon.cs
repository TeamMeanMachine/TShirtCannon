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

        private const int INDEXING_TURN_DELAY = 100;
        private const int INDEXING_RETRACT_DELAY = 25;
        private const int INDEXING_LATCH_DELAY = 75;
        //private const float INDEXING_LATCHED_CURRENT = 20f;

        public Cannon()
        {
            angleMotor.SetNeutralMode(NeutralMode.Brake);
            revolverMotor.SetNeutralMode(NeutralMode.Brake);
            angleMotor.ConfigPeakOutputForward(0.5f);
            angleMotor.ConfigPeakOutputReverse(-0.5f);

            revolverMotor.SetInverted(true);
            revolverMotor.ConfigPeakOutputForward(0.5f);
            revolverMotor.ConfigPeakOutputReverse(0.5f);

            revolverMotor.ConfigContinuousCurrentLimit(20);
            revolverMotor.ConfigPeakCurrentLimit(0);
            revolverMotor.ConfigPeakCurrentDuration(0);
        }

        public void Update(bool firing)
        {
            if (state == State.READY)
            {
                if (firing)
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
