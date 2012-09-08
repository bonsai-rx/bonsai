using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Arduino
{
    public class StepperConfiguration : SysexConfiguration
    {
        public int Stepper { get; set; }

        public int DirectionPin { get; set; }

        public int StepPin { get; set; }

        public int MaxSpeed { get; set; }

        public int Acceleration { get; set; }

        public override void Configure(Arduino arduino)
        {
            arduino.StepperConfig(Stepper, StepperMotorInterfaceType.Driver, StepPin, DirectionPin);
            arduino.StepperAcceleration(Stepper, MaxSpeed, Acceleration);
        }
    }
}
