using System;
using System.ComponentModel;
using System.Globalization;
using Rx = System.Reactive.Concurrency;

namespace Bonsai.Reactive.Concurrency
{
    internal class SchedulerMappingConverter : TypeConverter
    {
        static readonly SchedulerMapping[] DefaultSchedulers = new SchedulerMapping[]
        {
            new SchedulerMapping(),
            new SchedulerMapping { Instance = Rx.DefaultScheduler.Instance },
            new SchedulerMapping { Instance = Rx.CurrentThreadScheduler.Instance },
            new SchedulerMapping { Instance = Rx.ImmediateScheduler.Instance },
            new SchedulerMapping { Instance = Rx.ThreadPoolScheduler.Instance },
            new SchedulerMapping { Instance = Rx.NewThreadScheduler.Default },
            new SchedulerMapping { Instance = Rx.TaskPoolScheduler.Default },
        };

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string name)
            {
                return new SchedulerMapping { InstanceXml = name };
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is SchedulerMapping mapping && destinationType == typeof(string))
            {
                return mapping.InstanceXml;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(DefaultSchedulers);
        }
    }
}
