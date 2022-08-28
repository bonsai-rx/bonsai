using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    static class GraphHelper
    {
        static readonly ConstructorInfo NewPointPair = typeof(PointPair).GetConstructor(new[] { typeof(double), typeof(double) });

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
                FormatDateAxis(axis);
            }
            else if (type == typeof(string))
            {
                axis.Type = AxisType.Text;
                axis.MinorTic.IsAllTics = false;
                axis.ScaleFormatEvent += (graph, axis, value, index) =>
                {
                    if (graph.CurveList.Count == 0) return null;
                    var series = graph.CurveList[0];
                    index *= (int)axis.Scale.MajorStep;
                    return index < series.NPts ? series[index].Tag as string : null;
                };
            }
            else
            {
                axis.Type = AxisType.LinearAsOrdinal;
                if (type.IsPrimitive && IsIntegralType(type))
                {
                    axis.Scale.Format = "F0";
                    axis.MinorTic.IsAllTics = false;
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

        internal static Expression SelectIndexMember(Expression expression, string indexSelector, out string indexLabel)
        {
            Expression selectedIndex;
            if (!string.IsNullOrEmpty(indexSelector))
            {
                selectedIndex = ExpressionHelper.SelectMembers(expression, indexSelector).First();
                indexLabel = indexSelector;
            }
            else
            {
                selectedIndex = Expression.Property(null, typeof(DateTime), nameof(DateTime.Now));
                indexLabel = "Time";
            }

            if (selectedIndex.Type == typeof(DateTimeOffset)) selectedIndex = Expression.Property(selectedIndex, nameof(DateTimeOffset.DateTime));
            if (selectedIndex.Type == typeof(DateTime))
            {
                selectedIndex = Expression.Convert(selectedIndex, typeof(ZedGraph.XDate));
            }

            return selectedIndex;
        }

        internal static Expression SelectDataValues(Expression expression, string valueSelector, out string[] valueLabels)
        {
            Expression selectedValues;
            var memberNames = ExpressionHelper.SelectMemberNames(valueSelector).ToArray();
            if (memberNames.Length == 0) memberNames = new[] { ExpressionHelper.ImplicitParameterName };
            var selectedMembers = memberNames.Select(name => ExpressionHelper.MemberAccess(expression, name))
                .SelectMany(UnwrapMemberAccess)
                .Select(x => x.Type.IsArray ? x : Expression.Convert(x, typeof(double))).ToArray();
            if (selectedMembers.Length == 1 && selectedMembers[0].Type.IsArray)
            {
                valueLabels = null;
                selectedValues = ConvertArray(selectedMembers[0], typeof(double));
            }
            else
            {
                valueLabels = memberNames;
                selectedValues = Expression.NewArrayInit(typeof(double), selectedMembers);
            }

            return selectedValues;
        }

        internal static Expression SelectDataPoints(Expression expression, string valueSelector, out string[] valueLabels)
        {
            return SelectDataPoints(expression, valueSelector, out valueLabels, out _);
        }

        internal static Expression SelectDataPoints(Expression expression, string valueSelector, out string[] valueLabels, out bool labelAxes)
        {
            labelAxes = false;
            var memberNames = ExpressionHelper.SelectMemberNames(valueSelector).ToArray();
            if (memberNames.Length == 0) memberNames = new[] { ExpressionHelper.ImplicitParameterName };
            if (memberNames.Length == 1)
            {
                var memberName = memberNames[0];
                valueLabels = memberName != ExpressionHelper.ImplicitParameterName ? new[] { memberName } : null;
                var member = ExpressionHelper.MemberAccess(expression, memberNames[0]);
                if (member.Type.IsArray)
                {
                    if (member.Type != typeof(PointPair[]))
                    {
                        var arrayElement = Expression.Parameter(member.Type.GetElementType());
                        member = ConvertArray(member, GetPointPair(arrayElement), arrayElement);
                    }

                    return member;
                }
                else return Expression.NewArrayInit(typeof(PointPair), GetPointPair(member));
            }

            valueLabels = memberNames;
            var members = Array.ConvertAll(memberNames, name => ExpressionHelper.MemberAccess(expression, name));
            if (members.Length == 2 && members[0].Type.IsPrimitive && members[1].Type.IsPrimitive)
            {
                labelAxes = true;
                var x = Expression.Convert(members[0], typeof(double));
                var y = Expression.Convert(members[1], typeof(double));
                return Expression.NewArrayInit(typeof(PointPair), Expression.New(NewPointPair, x, y));
            }

            for (int i = 0; i < members.Length; i++)
            {
                members[i] = GetPointPair(members[i]);
            }
            return Expression.NewArrayInit(typeof(PointPair), members);
        }

        static Expression ConvertArray(Expression array, Type targetType)
        {
            var elementType = array.Type.GetElementType();
            if (elementType != targetType)
            {
                var arrayElement = Expression.Parameter(array.Type.GetElementType());
                var converterBody = Expression.Convert(arrayElement, targetType);
                array = ConvertArray(array, converterBody, arrayElement);
            }

            return array;
        }

        static Expression ConvertArray(Expression array, Expression body, ParameterExpression parameter)
        {
            var typeArguments = new[] { parameter.Type, body.Type };
            var converterType = typeof(Converter<,>).MakeGenericType(typeArguments);
            return Expression.Call(
                typeof(Array),
                nameof(Array.ConvertAll),
                typeArguments, array,
                Expression.Lambda(converterType, body, parameter));
        }

        static Expression GetDoubleMember(Expression expression, string memberName)
        {
            var member = (Expression)Expression.PropertyOrField(expression, memberName);
            return member.Type != typeof(double) ? Expression.Convert(member, typeof(double)) : member;
        }

        static Expression GetPointPair(Expression expression)
        {
            if (expression.Type == typeof(PointPair)) return expression;
            if (expression.Type == typeof(Tuple<,>))
            {
                return Expression.New(NewPointPair,
                    GetDoubleMember(expression, "Item1"),
                    GetDoubleMember(expression, "Item2"));
            }

            return Expression.New(NewPointPair,
                GetDoubleMember(expression, nameof(PointPair.X)),
                GetDoubleMember(expression, nameof(PointPair.Y)));
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

        static IEnumerable<Expression> UnwrapMemberAccess(Expression expression)
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
