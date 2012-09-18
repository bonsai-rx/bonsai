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
        public int Stepper { get; set; }

        public int DirectionPin { get; set; }

        public int StepPin { get; set; }

        public int? EnablePin { get; set; }

        [Browsable(false)]
        public bool EnablePinSpecified
        {
            get { return EnablePin.HasValue; }
        }

        public int MaxSpeed { get; set; }

        public int Acceleration { get; set; }

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
