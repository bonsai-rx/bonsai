using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Generates a sequence of fixed update microsteps matching the elapsed real time.")]
    public class UpdateStep : Combinator<double, double>
    {
        public UpdateStep()
        {
            StepSize = 1;
        }

        [Description("The size of each microstep, as a fraction of the refresh rate in case of window input, or in seconds in case of time input.")]
        public double StepSize { get; set; }

        [Description("The optional upper bound for elapsed time, used to allow running non-real time updates in slower systems.")]
        public double? MaxElapsedTime { get; set; }

        private static void Step(double stepSize, double? maxElapsedTime, ref double remainingTime, IObserver<double> observer)
        {
            if (remainingTime > maxElapsedTime)
            {
                remainingTime = maxElapsedTime.Value;
            }

            var stepCount = (int)Math.Floor(remainingTime / stepSize);
            for (int i = 0; i < stepCount; i++)
            {
                observer.OnNext(stepSize);
            }

            remainingTime -= stepCount * stepSize;
        }

        public IObservable<double> Process(IObservable<FrameEvent> source)
        {
            return Process(source.Select(evt => evt.TimeStep));
        }

        public IObservable<double> Process(IObservable<TimeStep> source)
        {
            return Observable.Create<double>(observer =>
            {
                var remainingTime = 0.0;
                var timeObserver = Observer.Create<TimeStep>(
                    step =>
                    {
                        var stepSize = step.ElapsedTime / StepSize;
                        if (stepSize <= 0)
                        {
                            throw new InvalidOperationException("The size of each update microstep must be a positive value.");
                        }

                        remainingTime += step.ElapsedRealTime;
                        Step(stepSize, MaxElapsedTime, ref remainingTime, observer);
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(timeObserver);
            });
        }

        public override IObservable<double> Process(IObservable<double> source)
        {
            return Observable.Create<double>(observer =>
            {
                var remainingTime = 0.0;
                return source.Do(elapsedTime =>
                {
                    var stepSize = StepSize;
                    if (stepSize <= 0)
                    {
                        throw new InvalidOperationException("The size of each update microstep must be a positive value.");
                    }

                    remainingTime += elapsedTime;
                    Step(stepSize, MaxElapsedTime, ref remainingTime, observer);
                })
                .IgnoreElements()
                .SubscribeSafe(observer);
            });
        }
    }
}
