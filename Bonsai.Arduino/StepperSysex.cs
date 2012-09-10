using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace Bonsai.Arduino
{
    public static class StepperSysex
    {
        const int SysexBitmask = 0x7F;
        const int SysexBitshift = 7;
        const byte STEPPER_COMMAND = 0x67; // send data for a digital port
        const byte STEPPER_CONFIG  = 0x01;
        const byte STEPPER_ACCEL  = 0x02;
        const byte STEPPER_MOVE    = 0x03;
        const byte STEPPER_MOVETO  = 0x04;

        public static void StepperConfig(this Arduino arduino, int device, StepperMotorInterfaceType interfaceType, params int[] pins)
        {
            var sysex = new byte[pins.Length + 3];
            sysex[0] = STEPPER_CONFIG;
            sysex[1] = (byte)device;
            sysex[2] = (byte)interfaceType;
            for (int i = 0; i < pins.Length; i++)
            {
                sysex[i + 3] = (byte)pins[i];
            }
            arduino.SendSysex(STEPPER_COMMAND, sysex);
        }

        public static void StepperAcceleration(this Arduino arduino, int device, int maxSpeed, int acceleration)
        {
            arduino.SendSysex(
                STEPPER_COMMAND,
                STEPPER_ACCEL,
                (byte)device,
                (byte)(maxSpeed & SysexBitmask),
                (byte)((maxSpeed >> SysexBitshift) & SysexBitmask),
                (byte)(acceleration & SysexBitmask),
                (byte)((acceleration >> SysexBitshift) & SysexBitmask));
        }

        public static void StepperMove(this Arduino arduino, int device, int relative)
        {
            arduino.SendSysex(
                STEPPER_COMMAND,
                STEPPER_MOVE,
                (byte)device,
                (byte)(relative & SysexBitmask),
                (byte)((relative >> SysexBitshift) & SysexBitmask));
        }

        public static void StepperMoveTo(this Arduino arduino, int device, int absolute)
        {
            arduino.SendSysex(
                STEPPER_COMMAND,
                STEPPER_MOVETO,
                (byte)device,
                (byte)(absolute & SysexBitmask),
                (byte)((absolute >> SysexBitshift) & SysexBitmask));
        }

        public static IObservable<int> StepperDone(this Arduino arduino)
        {
            return from evt in Observable.FromEventPattern<SysexReceivedEventArgs>(
                       handler => arduino.SysexReceived += handler,
                       handler => arduino.SysexReceived -= handler)
                   where evt.EventArgs.Command == STEPPER_COMMAND
                   select (int)evt.EventArgs.Args[0];
        }
    }

    public enum StepperMotorInterfaceType : byte
    {
        Function = 0,
        Driver = 1,
        Full2Wire = 2,
        Full4Wire = 4,
        Half4Wire = 8
    }
}
