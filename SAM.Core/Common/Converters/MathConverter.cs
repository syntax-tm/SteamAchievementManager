using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using JetBrains.Annotations;
using MahApps.Metro.Converters;

namespace SAM.Core.Converters
{
    /// <summary>
    /// The math operations which can be used at the <see cref="MathConverter"/>
    /// </summary>
    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>
    /// MathConverter provides a value converter which can be used for math operations.
    /// It can be used for normal binding or multi binding as well.
    /// If it is used for normal binding the given parameter will be used as operands with the selected operation.
    /// If it is used for multi binding then the first and second binding will be used as operands with the selected operation.
    /// This class cannot be inherited.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    public sealed class MathConverter : IValueConverter, IMultiValueConverter
    {
        public MathOperation Operation { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DoConvert(value, parameter, this.Operation, targetType);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values is null
                ? Binding.DoNothing
                : DoConvert(values.ElementAtOrDefault(0), values.ElementAtOrDefault(1), this.Operation, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => DependencyProperty.UnsetValue).ToArray();
        }

        private static object DoConvert([CanBeNull] object firstValue, [CanBeNull] object secondValue, MathOperation operation, Type targetType = null)
        {
            if (firstValue is null
                || secondValue is null
                || firstValue == DependencyProperty.UnsetValue
                || secondValue == DependencyProperty.UnsetValue
                || firstValue == DBNull.Value
                || secondValue == DBNull.Value)
            {
                return Binding.DoNothing;
            }
            
            try
            {
                var value1 = (firstValue as double?).GetValueOrDefault(System.Convert.ToDouble(firstValue, CultureInfo.InvariantCulture));
                var value2 = (secondValue as double?).GetValueOrDefault(System.Convert.ToDouble(secondValue, CultureInfo.InvariantCulture));

                Func<double> operationFunc;

                switch (operation)
                {
                    case MathOperation.Add:
                        operationFunc = () => value1 + value2;
                        break;
                    case MathOperation.Divide:
                    {
                        if (value2 > 0)
                        {
                            operationFunc = () => value1 / value2;
                            break;
                        }

                        Trace.TraceWarning($"Second value can not be used by division, because it's '0' (value1={value1}, value2={value2})");
                        return Binding.DoNothing;
                    }
                    case MathOperation.Multiply:
                        operationFunc = () => value1 * value2;
                        break;
                    case MathOperation.Subtract:
                        operationFunc = () => value1 - value2;
                        break;
                    default: return Binding.DoNothing;
                }

                var returnVal = operationFunc.Invoke();

                return targetType != null
                    ? System.Convert.ChangeType(returnVal, targetType)
                    : returnVal;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error while math operation: operation={operation}, value1={firstValue}, value2={secondValue} => exception: {e}");
                return Binding.DoNothing;
            }
        }
    }

    /// <summary>
    /// MathAddConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
    /// This class cannot be inherited.
    /// </summary>
    [MarkupExtensionReturnType(typeof(MathAddConverter))]
    public sealed class MathAddConverter : MarkupMultiConverter
    {
        private static readonly MathConverter MathConverter = new MathConverter { Operation = MathOperation.Add };

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(values, targetType, parameter, culture);
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(value, targetType, parameter, culture);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetTypes, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// MathSubtractConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
    /// This class cannot be inherited.
    /// </summary>
    [MarkupExtensionReturnType(typeof(MathSubtractConverter))]
    public sealed class MathSubtractConverter : MarkupMultiConverter
    {
        private static readonly MathConverter MathConverter = new MathConverter { Operation = MathOperation.Subtract };

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(values, targetType, parameter, culture);
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(value, targetType, parameter, culture);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetTypes, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// MathMultiplyConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
    /// This class cannot be inherited.
    /// </summary>
    [MarkupExtensionReturnType(typeof(MathMultiplyConverter))]
    public sealed class MathMultiplyConverter : MarkupMultiConverter
    {
        private static readonly MathConverter MathConverter = new MathConverter { Operation = MathOperation.Multiply };

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(values, targetType, parameter, culture);
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(value, targetType, parameter, culture);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetTypes, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }

    /// <summary>
    /// MathDivideConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
    /// This class cannot be inherited.
    /// </summary>
    [MarkupExtensionReturnType(typeof(MathDivideConverter))]
    public sealed class MathDivideConverter : MarkupMultiConverter
    {
        private static readonly MathConverter MathConverter = new MathConverter { Operation = MathOperation.Divide };

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(values, targetType, parameter, culture);
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.Convert(value, targetType, parameter, culture);
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetTypes, parameter, culture);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return MathConverter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
