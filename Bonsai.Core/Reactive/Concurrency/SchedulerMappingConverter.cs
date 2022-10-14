using System;
using System.ComponentModel;
using System.Globalization;

namespace Bonsai.Reactive.Concurrency
{
    internal class SchedulerMappingConverter : TypeConverter
    {
        static readonly SchedulerMapping[] DefaultSchedulers = new SchedulerMapping[]
        {
            new DefaultScheduler(),
            new CurrentThreadScheduler(),
            new ImmediateScheduler(),
            new NewThreadScheduler(),
            new TaskPoolScheduler(),
            new ThreadPoolScheduler()
        };

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string name)
            {
                var scheduler = Array.Find(DefaultSchedulers, x => x?.GetType().Name == (string)value);
                return scheduler ?? throw new ArgumentException(
                    "The specified string does not identify a well-known scheduler type.",
                    nameof(value));
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                var scheduler = Array.Find(DefaultSchedulers, x => x != null && x.Equals(value));
                if (scheduler == null)
                {
                    return "(" + nameof(SchedulerMapping) + ")";
                }

                var sourceType = value.GetType();
                return sourceType.Name;
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
