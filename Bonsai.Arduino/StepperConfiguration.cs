using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Arduino
{
    public class StepperConfiguration : SysexConfiguration
    {
        [Category("Data")]
        [Description("The index of the stepper motor to configure.")]
        public int Stepper { get; set; }

        [Description("The digital output pin used by the stepper motor driver to determine direction.")]
        public int DirectionPin { get; set; }

        [Description("The digital output pin used by the stepper motor driver to receive step commands.")]
        public int StepPin { get; set; }

        [Description("The optional digital output pin used to cut/restore power to the stepper motor.")]
        public int? EnablePin { get; set; }

        [Browsable(false)]
        public bool EnablePinSpecified
        {
            get { return EnablePin.HasValue; }
        }

        [Description("The maximum speed of the stepper motor.")]
        public int MaxSpeed { get; set; }

        [Description("The acceleration value of the stepper motor.")]
        public int Acceleration { get; set; }

        [Description("The total number of steps in a single revolution of the motor for angular position control.")]
        public int StepsPerRevolution { get; set; }

        public override void Configure(Arduino arduino)
        {
            arduino.StepperConfig(Stepper, StepsPerRevolution, StepperMotorInterfaceType.Driver, StepPin, DirectionPin);
            arduino.StepperParameters(Stepper, MaxSpeed, Acceleration);
            if (EnablePinSpecified)
            {
                arduino.PinMode(EnablePin.Value, PinMode.Output);
            }
        }
    }
}
