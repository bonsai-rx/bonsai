using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive;
using System.Reactive.Disposables;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that generate expression tree nodes
    /// are derived. This is an abstract class.
    /// </summary>
    [XmlInclude(typeof(UnitBuilder))]
    [XmlInclude(typeof(SourceBuilder))]
    [XmlInclude(typeof(ConditionBuilder))]
    [XmlInclude(typeof(CombinatorBuilder))]
    [XmlInclude(typeof(SelectManyBuilder))]
    [XmlInclude(typeof(PublishBuilder))]
    [XmlInclude(typeof(ReplayBuilder))]
    [XmlInclude(typeof(WindowWorkflowBuilder))]
    [XmlInclude(typeof(NestedWorkflowBuilder))]
    [XmlInclude(typeof(MemberSelectorBuilder))]
    [XmlInclude(typeof(WorkflowInputBuilder))]
    [XmlInclude(typeof(WorkflowOutputBuilder))]
    [XmlInclude(typeof(WorkflowExpressionBuilder))]
    [XmlType("Expression", Namespace = Constants.XmlNamespace)]
    [TypeConverter("Bonsai.Design.ExpressionBuilderTypeConverter, Bonsai.Design")]
    public abstract class ExpressionBuilder : IExpressionBuilder
    {
        const string ExpressionBuilderSuffix = "Builder";

        /// <summary>
        /// When overridden in a derived class, gets the range of input arguments
        /// that this expression builder accepts.
        /// </summary>
        [Browsable(false)]
        public abstract Range<int> ArgumentRange { get; }

        /// <summary>
        /// When overridden in a derived class, generates an <see cref="Expression"/> node
        /// from a collection of input arguments. The result can be chained with other
        /// builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public abstract Expression Build(IEnumerable<Expression> arguments);

        /// <summary>
        /// Removes all decorators from a specified <see cref="ExpressionBuilder"/> instance
        /// and returns the first non-decorated (i.e. primitive) builder to be retrieved.
        /// </summary>
        /// <param name="builder">
        /// An <see cref="ExpressionBuilder"/> instance from which to remove decorators.
        /// </param>
        /// <returns>
        /// The first non-decorated <see cref="ExpressionBuilder"/> instance that is retrieved.
        /// </returns>
        public static ExpressionBuilder Unwrap(ExpressionBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var inspectBuilder = builder as InspectBuilder;
            if (inspectBuilder != null) return Unwrap(inspectBuilder.Builder);

            return builder;
        }

        /// <summary>
        /// Returns the editor browsable element for the specified <see cref="ExpressionBuilder"/>.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ExpressionBuilder"/> for which to retrieve the editor browsable element.
        /// </param>
        /// <returns>
        /// An <see cref="Object"/> that is the editor browsable element for the specified
        /// <paramref name="builder"/>.
        /// </returns>
        public static object GetWorkflowElement(ExpressionBuilder builder)
        {
            builder = Unwrap(builder);

            var sourceBuilder = builder as SourceBuilder;
            if (sourceBuilder != null) return sourceBuilder.Generator;

            var combinatorBuilder = builder as CombinatorBuilder;
            if (combinatorBuilder != null) return combinatorBuilder.Combinator;

            return builder;
        }

        /// <summary>
        /// Creates a new expression builder from the specified editor browsable element and category.
        /// </summary>
        /// <param name="element">The editor browsable element for which to build a new expression builder.</param>
        /// <param name="elementCategory">The workflow category of the specified element.</param>
        /// <returns>A new <see cref="ExpressionBuilder"/> object.</returns>
        public static ExpressionBuilder FromWorkflowElement(object element, ElementCategory elementCategory)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            var elementType = element.GetType();
            if (elementType.IsDefined(typeof(CombinatorAttribute), true))
            {
                return new CombinatorBuilder { Combinator = element };
            }

            if (elementCategory == ElementCategory.Source ||
                elementCategory == ElementCategory.Property)
            {
                return new SourceBuilder { Generator = element };
            }

            throw new InvalidOperationException("Invalid loadable element type.");
        }

        static string RemoveSuffix(string source, string suffix)
        {
            var suffixStart = source.LastIndexOf(suffix);
            return suffixStart >= 0 ? source.Remove(suffixStart) : source;
        }

        /// <summary>
        /// Gets the display name for the specified type.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which to retrieve the display name.</param>
        /// <returns>The display name for the specified <paramref name="type"/>.</returns>
        public static string GetElementDisplayName(Type type)
        {
            var displayNameAttribute = (DisplayNameAttribute)Attribute.GetCustomAttribute(type, typeof(DisplayNameAttribute));
            if (displayNameAttribute != null)
            {
                return displayNameAttribute.DisplayName;
            }

            return type.IsSubclassOf(typeof(ExpressionBuilder))
                ? RemoveSuffix(type.Name, ExpressionBuilderSuffix)
                : type.Name;
        }

        /// <summary>
        /// Gets the display name for the specified element.
        /// </summary>
        /// <param name="element">The element for which to retrieve the display name.</param>
        /// <returns>The name of the element.</returns>
        public static string GetElementDisplayName(object element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("component");
            }

            var namedElement = element as INamedElement;
            if (namedElement != null)
            {
                var name = namedElement.Name;
                if (!string.IsNullOrEmpty(name)) return name;
            }

            var componentType = element.GetType();
            return GetElementDisplayName(componentType);
        }

        #region Type Inference

        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        static Type[] GetMethodTypeArguments(MethodInfo methodInfo, params Type[] arguments)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (arguments == null)
            {
                throw new ArgumentNullException("arguments");
            }

            var methodParameters = methodInfo.GetParameters();
            var methodGenericArguments = methodInfo.GetGenericArguments();

            // The binding candidates are the distinct results from matching parameters with input
            // Matches for the same generic parameter position should be identical
            var bindingCandidates = (from bindings in methodParameters.Zip(arguments, (methodParameter, parameter) => GetParameterBindings(methodParameter.ParameterType, parameter))
                                     from binding in bindings
                                     group binding by binding.Item2 into matches
                                     orderby matches.Key ascending
                                     select matches.Distinct().Single().Item1)
                                     .ToArray();

            return methodGenericArguments.Zip(bindingCandidates, (argument, match) => match).Concat(methodGenericArguments.Skip(bindingCandidates.Length)).ToArray();
        }

        static IEnumerable<Tuple<Type, int>> GetParameterBindings(Type parameterType, Type argumentType)
        {
            // If parameter is a generic parameter, just bind it to the input type
            if (parameterType.IsGenericParameter)
            {
                return Enumerable.Repeat(Tuple.Create(argumentType, parameterType.GenericParameterPosition), 1);
            }
            // If parameter contains generic parameters, we may have possible bindings
            else if (parameterType.ContainsGenericParameters)
            {
                // Check if we have a straight type match
                var bindings = MatchTypeBindings(parameterType, argumentType).ToArray();
                if (bindings.Length > 0) return bindings;

                // Direct match didn't produce any bindings, so we need to check inheritance chain
                Type currentType = argumentType;
                while (currentType != typeof(object))
                {
                    currentType = currentType.BaseType;
                    if (currentType == null) break;
                    bindings = MatchTypeBindings(parameterType, currentType).ToArray();
                    if (bindings.Length > 0) return bindings;
                }

                // Inheritance chain match didn't produce any bindings, so we need to check interface set
                var interfaces = argumentType.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    bindings = MatchTypeBindings(parameterType, interfaceType).ToArray();
                    if (bindings.Length > 0) return bindings;
                }
            }

            // If parameter does not contain generic parameters, there's nothing to bind to (check for error?)
            return Enumerable.Empty<Tuple<Type, int>>();
        }

        static IEnumerable<Tuple<Type, int>> MatchTypeBindings(Type parameterType, Type argumentType)
        {
            // If both types have element types, try to recurse into them
            if (parameterType.HasElementType && argumentType.HasElementType)
            {
                if (parameterType.IsArray && !argumentType.IsArray ||
                    parameterType.IsPointer && !argumentType.IsPointer ||
                    parameterType.IsByRef && !argumentType.IsByRef)
                {
                    return Enumerable.Empty<Tuple<Type, int>>();
                }

                var parameterElementType = parameterType.GetElementType();
                var argumentElementType = argumentType.GetElementType();
                return MatchTypeBindings(parameterElementType, argumentElementType);
            }

            // Match bindings can only be obtained if both types are generic types
            if (parameterType.IsGenericType && argumentType.IsGenericType)
            {
                var parameterTypeDefinition = parameterType.GetGenericTypeDefinition();
                var argumentTypeDefinition = parameterType.GetGenericTypeDefinition();
                // Match bindings can only be obtained if both types share the same type definition
                if (parameterTypeDefinition == argumentTypeDefinition)
                {
                    var parameterGenericArguments = parameterType.GetGenericArguments();
                    var argumentGenericArguments = argumentType.GetGenericArguments();
                    return parameterGenericArguments
                        .Zip(argumentGenericArguments, (parameter, argument) => GetParameterBindings(parameter, argument))
                        .SelectMany(xs => xs);
                }
            }

            return Enumerable.Empty<Tuple<Type, int>>();
        }

        static bool MatchParamArrayTypeReferences(Type parameter, Type argument)
        {
            if (parameter.HasElementType && argument.HasElementType)
            {
                return MatchParamArrayTypeReferences(parameter.GetElementType(), argument.GetElementType());
            }

            return parameter.HasElementType == argument.HasElementType;
        }

        static bool ParamExpansionRequired(ParameterInfo[] parameters, Type[] arguments)
        {
            var offset = parameters.Length - 1;
            var paramArray = parameters.Length > 0 &&
                parameters[offset].ParameterType.IsArray &&
                Attribute.IsDefined(parameters[offset], typeof(ParamArrayAttribute));

            return paramArray &&
                (parameters.Length != arguments.Length ||
                 !MatchParamArrayTypeReferences(parameters[offset].ParameterType, arguments[arguments.Length - 1]));
        }

        static MethodInfo MakeGenericMethod(MethodInfo method, params Type[] argumentTypes)
        {
            if (!method.IsGenericMethodDefinition)
            {
                throw new ArgumentException("The specified method is not a generic definition.");
            }

            var methodCallArgumentTypes = (Type[])argumentTypes.Clone();
            var methodParameters = method.GetParameters();
            if (ParamExpansionRequired(methodParameters, methodCallArgumentTypes))
            {
                var arrayType = methodCallArgumentTypes[methodCallArgumentTypes.Length - 1].MakeArrayType();
                Array.Resize(ref methodCallArgumentTypes, methodParameters.Length);
                methodCallArgumentTypes[methodCallArgumentTypes.Length - 1] = arrayType;
            }

            var typeArguments = GetMethodTypeArguments(method, methodCallArgumentTypes);
            var genericArguments = method.GetGenericArguments();
            return genericArguments.Length == typeArguments.Length ? method.MakeGenericMethod(typeArguments) : method;
        }

        static bool CanExpandParamArguments(ParameterInfo[] parameters, Type[] argumentTypes)
        {
            var offset = parameters.Length - 1;
            var arrayType = parameters[offset].ParameterType.GetElementType();
            for (int i = offset; i < argumentTypes.Length; i++)
            {
                if (argumentTypes[i] != arrayType)
                {
                    return false;
                }
            }

            return true;
        }

        static Expression[] ExpandParamArguments(ParameterInfo[] parameters, Expression[] arguments)
        {
            var offset = parameters.Length - 1;
            var arrayType = parameters[offset].ParameterType.GetElementType();
            var initializers = new Expression[arguments.Length - offset];
            for (int k = 0; k < initializers.Length; k++)
            {
                if (arguments[k + offset].Type != arrayType)
                {
                    throw new InvalidOperationException("Parameter expansion requires tail arguments to be of the same type.");
                }
                initializers[k] = arguments[k + offset];
            }
            var paramArray = Expression.NewArrayInit(arrayType, initializers);
            Array.Resize(ref arguments, parameters.Length);
            arguments[arguments.Length - 1] = paramArray;
            return arguments;
        }

        static Expression[] MatchMethodParameters(ParameterInfo[] parameters, Expression[] arguments)
        {
            int i = 0;
            return Array.ConvertAll(arguments, argument =>
            {
                var parameterType = parameters[i++].ParameterType;
                if (argument.Type != parameterType)
                {
                    if (argument.Type.IsGenericType && parameterType.IsGenericType &&
                        argument.Type.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                        parameterType.GetGenericTypeDefinition() == typeof(IObservable<>))
                    {
                        var argumentObservableType = argument.Type.GetGenericArguments()[0];
                        var parameterObservableType = parameterType.GetGenericArguments()[0];
                        var conversionParameter = Expression.Parameter(argumentObservableType);
                        var conversion = Expression.Convert(conversionParameter, parameterObservableType);
                        var select = selectMethod.MakeGenericMethod(argumentObservableType, parameterObservableType);
                        return Expression.Call(select, argument, Expression.Lambda(conversion, conversionParameter));
                    }
                    else if (argument.Type.IsPrimitive)
                    {
                        return Expression.Convert(argument, parameterType);
                    }
                }
                return argument;
            });
        }

        #endregion

        #region Method Validation

        static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        static Type GetNonNullableType(Type type)
        {
            return IsNullable(type) ? type.GetGenericArguments()[0] : type;
        }

        static bool IsConvertiblePrimitive(Type type)
        {
            var nonNullableType = GetNonNullableType(type);
            if (nonNullableType == typeof(bool)) return false;
            if (nonNullableType.IsEnum) return true;
            return nonNullableType.IsPrimitive;
        }

        static bool HasPrimitiveConversion(Type from, Type to)
        {
            if (from == to) return true;
            if (IsNullable(from) && GetNonNullableType(from) == to) return true;
            if (IsNullable(to) && GetNonNullableType(to) == from) return true;
            return IsConvertiblePrimitive(from) && IsConvertiblePrimitive(to);
        }

        static bool HasReferenceConversion(Type from, Type to)
        {
            if (from == to) return true;
            var nonNullableFrom = GetNonNullableType(from);
            var nonNullableTo = GetNonNullableType(to);

            if (nonNullableFrom.IsAssignableFrom(nonNullableTo)) return true;
            if (nonNullableTo.IsAssignableFrom(nonNullableFrom)) return true;
            if (from.IsInterface || to.IsInterface) return true;
            return from == typeof(object) || to == typeof(object);
        }

        static MethodInfo GetUnaryOperator(Type type, string name, Type parameterType, Type returnType)
        {
            var methods = GetNonNullableType(type).GetMethods(PublicStaticBinding);
            foreach (var method in methods)
            {
                if (method.Name != name || method.IsGenericMethod || method.ReturnType != returnType) continue;

                var parameters = method.GetParameters();
                if (parameters.Length != 1) continue;

                if (parameters[0].ParameterType.IsAssignableFrom(GetNonNullableType(parameterType)))
                {
                    return method;
                }
            }

            return null;
        }

        static MethodInfo GetUserConversion(Type from, Type to)
        {
            var conversion = GetUnaryOperator(from, "op_Implicit", from, to);
            conversion = conversion ?? GetUnaryOperator(from, "op_Explicit", from, to);
            conversion = conversion ?? GetUnaryOperator(to, "op_Implicit", from, to);
            return conversion ?? GetUnaryOperator(to, "op_Explicit", from, to);
        }

        static bool HasConversion(Type from, Type to)
        {
            return HasPrimitiveConversion(from, to) || HasReferenceConversion(from, to) ||
                   GetUserConversion(from, to) != null;
        }

        static bool CanMatchMethodParameters(ParameterInfo[] parameters, Expression[] arguments)
        {
            int i = 0;
            if (parameters.Length != arguments.Length) return false;
            return Array.TrueForAll(arguments, argument =>
            {
                var parameterType = parameters[i++].ParameterType;
                if (argument.Type != parameterType)
                {
                    if (argument.Type.IsGenericType && parameterType.IsGenericType &&
                        argument.Type.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                        parameterType.GetGenericTypeDefinition() == typeof(IObservable<>))
                    {
                        var argumentObservableType = argument.Type.GetGenericArguments()[0];
                        var parameterObservableType = parameterType.GetGenericArguments()[0];
                        return HasConversion(argumentObservableType, parameterObservableType);
                    }

                    return HasConversion(argument.Type, parameterType);
                }

                return true;
            });
        }

        #endregion

        #region Overload Resolution

        static readonly BindingFlags PublicStaticBinding = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        static readonly Dictionary<Type, Type[]> ImplicitNumericConversions = new Dictionary<Type, Type[]>
        {
            { typeof(sbyte), new[] { typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(byte), new[] { typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(short), new[] { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(ushort), new[] { typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(int), new[] { typeof(long), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(uint), new[] { typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(long), new[] { typeof(float), typeof(double), typeof(decimal) } },
            { typeof(char), new[] { typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) } },
            { typeof(float), new[] { typeof(double) } },
            { typeof(ulong), new[] { typeof(float), typeof(double), typeof(decimal) } }
        };

        static bool HasImplicitConversion(Type from, Type to)
        {
            if (to.IsAssignableFrom(from)) return true;

            Type[] conversions;
            if (ImplicitNumericConversions.TryGetValue(from, out conversions))
            {
                return Array.Exists(conversions, type => type == to);
            }

            return from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                       .Any(m => m.ReturnType == to && m.Name == "op_Implicit");
        }

        internal static int CompareConversion(Type t1, Type t2, Type s)
        {
            if (t1 == t2) return 0;
            if (s == t1) return -1;
            if (s == t2) return 1;

            var implicitT1T2 = HasImplicitConversion(t1, t2);
            var implicitT2T1 = HasImplicitConversion(t2, t1);
            if (implicitT1T2 && !implicitT2T1) return -1;
            if (implicitT2T1 && !implicitT1T2) return 1;

            var t1Code = Type.GetTypeCode(t1);
            var t2Code = Type.GetTypeCode(t2);
            if (t1Code == TypeCode.SByte &&
                (t2Code == TypeCode.Byte || t2Code == TypeCode.UInt16 ||
                 t2Code == TypeCode.UInt32 || t2Code == TypeCode.UInt64)) return -1;

            if (t2Code == TypeCode.SByte &&
                (t1Code == TypeCode.Byte || t1Code == TypeCode.UInt16 ||
                 t1Code == TypeCode.UInt32 || t1Code == TypeCode.UInt64)) return 1;

            if (t1Code == TypeCode.Int16 &&
                (t2Code == TypeCode.UInt16 || t2Code == TypeCode.UInt32 || t2Code == TypeCode.UInt64)) return -1;
            if (t2Code == TypeCode.Int16 &&
                (t1Code == TypeCode.UInt16 || t1Code == TypeCode.UInt32 || t1Code == TypeCode.UInt64)) return 1;

            if (t1Code == TypeCode.Int32 && (t2Code == TypeCode.UInt32 || t2Code == TypeCode.UInt64)) return -1;
            if (t2Code == TypeCode.Int32 && (t1Code == TypeCode.UInt32 || t1Code == TypeCode.UInt64)) return 1;
            if (t1Code == TypeCode.Int64 && t2Code == TypeCode.UInt64) return -1;
            if (t2Code == TypeCode.Int64 && t1Code == TypeCode.UInt64) return 1;
            return 0;
        }

        static int CompareFunctionMember(Type[] parametersA, Type[] parametersB, Type[] arguments)
        {
            bool? betterA = null;
            bool? betterB = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var comparison = CompareConversion(parametersA[i], parametersB[i], arguments[i]);
                if (comparison < 0)
                {
                    if (!betterA.HasValue) betterA = true;
                    betterB = false;
                }
                else if (comparison > 0)
                {
                    if (!betterB.HasValue) betterB = true;
                    betterA = false;
                }
            }

            if (betterA.GetValueOrDefault()) return -1;
            if (betterB.GetValueOrDefault()) return 1;
            return 0;
        }

        static Type[] ExpandCallParameterTypes(ParameterInfo[] parameters, Type[] arguments, bool expansion)
        {
            var expandedParameters = new Type[arguments.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                expandedParameters[i] = parameters[i].ParameterType;
            }

            if (expansion)
            {
                for (int i = parameters.Length-1; i < expandedParameters.Length; i++)
                {
                    expandedParameters[i] = parameters[parameters.Length - 1].ParameterType.GetElementType();
                }
            }

            return expandedParameters;
        }

        internal static Expression BuildCall(Expression instance, IEnumerable<MethodInfo> methods, params Expression[] arguments)
        {
            var argumentTypes = Array.ConvertAll(arguments, argument => argument.Type);
            var candidates = methods
                .Where(method =>
                {
                    var parameters = method.GetParameters();
                    return parameters.Length == arguments.Length ||
                        parameters.Length > 0 && arguments.Length >= (parameters.Length - 1) &&
                        Attribute.IsDefined(parameters[parameters.Length - 1], typeof(ParamArrayAttribute));
                })
                .Select(method =>
                {
                    MethodCallExpression call;
                    try
                    {
                        if (method.IsGenericMethodDefinition)
                        {
                            method = MakeGenericMethod(method, argumentTypes);
                            if (method.IsGenericMethodDefinition) return null;
                        }

                        var parameters = method.GetParameters();
                        if (ParamExpansionRequired(parameters, argumentTypes))
                        {
                            if (!CanExpandParamArguments(parameters, argumentTypes)) return null;
                            arguments = ExpandParamArguments(parameters, arguments);
                        }

                        if (!CanMatchMethodParameters(parameters, arguments)) return null;
                        arguments = MatchMethodParameters(parameters, arguments);
                        call = Expression.Call(instance, method, arguments);
                    }
                    catch (ArgumentException) { return null; }
                    catch (InvalidOperationException) { return null; }
                    return new
                    {
                        call,
                        generic = call.Method != method,
                        expansion = ParamExpansionRequired(call.Method.GetParameters(), argumentTypes)
                    };
                })
                .Where(candidate => candidate != null)
                .ToArray();

            if (candidates.Length == 0)
            {
                throw new InvalidOperationException("No method overload found for the given arguments.");
            }

            if (candidates.Length == 1) return candidates[0].call;

            int best = -1;
            var candidateParameters = Array.ConvertAll(
                candidates,
                candidate => ExpandCallParameterTypes(candidate.call.Method.GetParameters(), argumentTypes, candidate.expansion));

            for (int i = 0; i < candidateParameters.Length;)
            {
                for (int j = 0; j < candidateParameters.Length; j++)
                {
                    if (i == j) continue;
                    var comparison = CompareFunctionMember(
                        candidateParameters[i],
                        candidateParameters[j],
                        argumentTypes);

                    int oldBest = -1;
                    if (best >= 0) oldBest = best;
                    if (comparison < 0) best = i;
                    if (comparison > 0) best = j;
                    if (comparison == 0)
                    {
                        var tie = true;
                        if (!candidates[i].generic && candidates[j].generic) { best = i; tie = false; }
                        if (!candidates[j].generic && candidates[i].generic) { best = j; tie = false; }

                        if (tie)
                        {
                            if (!candidates[i].expansion && candidates[j].expansion) best = i;
                            if (!candidates[j].expansion && candidates[i].expansion) best = j;
                        }
                    }

                    if (best != oldBest && oldBest > 0)
                    {
                        best = -1;
                        break;
                    }

                    if (best == j) break;
                }

                if (best < 0) break;
                if (best == i) break;
                i = best;
            }

            if (best < 0) throw new InvalidOperationException("The method overload call is ambiguous.");
            return candidates[best].call;
        }

        #endregion

        #region Nested Workflow Output

        static IObservable<Unit> IgnoreConnection<TSource>(IObservable<TSource> source)
        {
            return source.IgnoreElements().Select(xs => Unit.Default);
        }

        static IObservable<Unit> MergeOutput(params IObservable<Unit>[] connections)
        {
            return Observable.Merge(connections);
        }

        static IObservable<TSource> MergeOutput<TSource>(IObservable<TSource> source, params IObservable<Unit>[] connections)
        {
            return source.Publish(ps => ps.Merge(Observable.Merge(connections).Select(xs => default(TSource)).TakeUntil(ps.TakeLast(1))));
        }

        internal static Expression BuildOutput(Expression output, IEnumerable<Expression> connections)
        {
            var ignoredConnections = from connection in connections
                                     where connection != output
                                     let observableType = connection.Type.GetGenericArguments()[0]
                                     select Expression.Call(typeof(ExpressionBuilder), "IgnoreConnection", new[] { observableType }, connection);

            var connectionArrayExpression = Expression.NewArrayInit(typeof(IObservable<Unit>), ignoredConnections.ToArray());
            if (output != null)
            {
                var outputType = output.Type.GetGenericArguments()[0];
                return Expression.Call(typeof(ExpressionBuilder), "MergeOutput", new[] { outputType }, output, connectionArrayExpression);
            }
            else return Expression.Call(typeof(ExpressionBuilder), "MergeOutput", null, connectionArrayExpression);
        }

        internal static Expression BuildWorkflowOutput(Expression workflowOutput, IEnumerable<Expression> connections)
        {
            var output = workflowOutput != null ? connections.FirstOrDefault(connection => connection == workflowOutput) : null;
            return BuildOutput(output, connections);
        }

        #endregion

        #region Error Handling

        static readonly ConstructorInfo buildExceptionConstructor = typeof(WorkflowBuildException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo throwMethod = typeof(Observable).GetMethods()
                                                                   .Where(m => m.Name == "Throw")
                                                                   .Single(m => m.GetParameters().Length == 1);

        internal static Expression HandleBuildException(Expression expression, ExpressionBuilder builder)
        {
            var exceptionVariable = Expression.Variable(typeof(Exception));
            var observableType = expression.Type.GetGenericArguments()[0];
            return Expression.TryCatch(
                expression,
                Expression.Catch(
                    exceptionVariable,
                    Expression.Call(
                        throwMethod.MakeGenericMethod(observableType),
                        Expression.New(
                            buildExceptionConstructor,
                            Expression.Property(exceptionVariable, "Message"),
                            Expression.Constant(builder),
                            exceptionVariable))));
        }

        #endregion

        #region Dynamic Properties

        static readonly ConstructorInfo compositeDisposableConstructor = typeof(CompositeDisposable).GetConstructor(new[] { typeof(IDisposable[]) });
        static readonly MethodInfo subscribeMethod = typeof(ObservableExtensions).GetMethods()
                                                                                 .Single(m => m.Name == "Subscribe" &&
                                                                                         m.GetParameters().Length == 2 &&
                                                                                         m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                         m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));
        static readonly MethodInfo usingMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Using" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(Func<>) &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        protected Tuple<Expression, string> GetArgumentAccess(IEnumerable<Expression> arguments, string selector)
        {
            if (string.IsNullOrEmpty(selector))
            {
                selector = ExpressionBuilderArgument.ArgumentNamePrefix;
            }

            var memberPath = selector.Split(new[] { ExpressionHelper.MemberSeparator }, 2, StringSplitOptions.None);
            var argumentName = memberPath[0];
            var argument = new ExpressionBuilderArgument(argumentName);
            var source = arguments.ElementAtOrDefault(argument.Index);
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("Unable to find source with name '{0}'.", argumentName));
            }

            selector = memberPath.Length > 1 ? memberPath[1] : string.Empty;
            return Tuple.Create(source, selector);
        }

        internal Expression BuildPropertyMapping(IEnumerable<Expression> arguments, Expression instance, PropertyMapping mapping)
        {
            var memberAccess = GetArgumentAccess(arguments, mapping.Selector);
            var source = memberAccess.Item1;
            var sourceSelector = memberAccess.Item2;
            var sourceType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(sourceType);
            var body = ExpressionHelper.MemberAccess(parameter, sourceSelector);

            var actionType = Expression.GetActionType(parameter.Type);
            var property = Expression.Property(instance, mapping.Name);
            if (body.Type != property.Type)
            {
                body = Expression.Convert(body, property.Type);
            }

            body = Expression.Assign(property, body);
            var action = Expression.Lambda(actionType, body, parameter);
            return Expression.Call(subscribeMethod.MakeGenericMethod(sourceType), source, action);
        }

        internal Expression BuildMappingOutput(IEnumerable<Expression> arguments, Expression instance, Expression output, PropertyMappingCollection propertyMappings)
        {
            var subscriptions = propertyMappings.Select(mapping => BuildPropertyMapping(arguments, instance, mapping)).ToArray();
            return BuildMappingOutput(output, subscriptions);
        }

        internal Expression BuildMappingOutput(Expression output, params Expression[] mappings)
        {
            if (mappings.Length > 0)
            {
                var outputType = output.Type.GetGenericArguments()[0];
                var resource = Expression.New(compositeDisposableConstructor, Expression.NewArrayInit(typeof(IDisposable), mappings));
                var resourceFactory = Expression.Lambda(resource);
                var resourceParameter = Expression.Parameter(resource.Type);
                var observableFactory = Expression.Lambda(output, resourceParameter);
                return Expression.Call(usingMethod.MakeGenericMethod(outputType, resource.Type), resourceFactory, observableFactory);
            }

            return output;
        }

        #endregion
    }
}
