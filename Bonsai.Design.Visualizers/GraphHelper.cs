using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    static class GraphHelper
    {
        internal static void SetAxisLabel(Axis axis, string label)
        {
            axis.Title.Text = label;
            axis.Title.IsVisible = !string.IsNullOrEmpty(label);
        }

        internal static void FormatDateAxis(Axis axis)
        {
            axis.Type = AxisType.DateAsOrdinal;
            axis.Scale.Format = "HH:mm:ss";
            axis.Scale.MajorUnit = DateUnit.Second;
            axis.Scale.MinorUnit = DateUnit.Millisecond;
            axis.MinorTic.IsAllTics = false;
        }

        internal static void FormatOrdinalAxis(Axis axis, Type type)
        {
            if (type == typeof(XDate))
            {
                GraphHelper.FormatDateAxis(axis);
            }
            else
            {
                axis.Type = AxisType.LinearAsOrdinal;
                if (type.IsPrimitive && IsIntegralType(type))
                {
                    axis.Scale.Format = "F0";
                }
                else axis.Scale.Format = "F2";
            }
        }

        static bool IsIntegralType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        static IEnumerable<MemberInfo> GetDataMembers(Type type)
        {
            var members = Enumerable.Concat<MemberInfo>(
                type.GetFields(BindingFlags.Instance | BindingFlags.Public),
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            if (type.IsInterface)
            {
                members = members.Concat(type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties(BindingFlags.Instance | BindingFlags.Public)));
            }
            return members.Where(member => !member.IsDefined(typeof(XmlIgnoreAttribute), true))
                          .OrderBy(member => member.MetadataToken)
                          .Except(type.GetDefaultMembers());
        }

        internal static IEnumerable<Expression> UnwrapMemberAccess(Expression expression)
        {
            if (expression.Type == typeof(string) || IsNullable(expression.Type)) yield return expression;
            else if (expression.Type.IsPrimitive || expression.Type.IsEnum || expression.Type.IsArray ||
                     expression.Type == typeof(string) || expression.Type == typeof(TimeSpan) ||
                     expression.Type == typeof(DateTime) || expression.Type == typeof(DateTimeOffset))
            {
                yield return expression;
            }
            else
            {
                var successors = from member in GetDataMembers(expression.Type)
                                 let memberAccess = (Expression)Expression.MakeMemberAccess(expression, member)
                                 select memberAccess;
                foreach (var successor in successors)
                {
                    yield return successor;
                }
            }
        }

        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
