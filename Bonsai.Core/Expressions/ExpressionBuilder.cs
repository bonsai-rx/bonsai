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
using System.Reactive.Concurrency;
using System.Threading;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that generate expression tree nodes
    /// are derived. This is an abstract class.
    /// </summary>
    [XmlType("Expression", Namespace = Constants.XmlNamespace)]
    [TypeConverter("Bonsai.Design.ExpressionBuilderTypeConverter, Bonsai.Design")]
    public abstract class ExpressionBuilder : IExpressionBuilder
    {
        const string ExpressionBuilderSuffix = "Builder";
        internal readonly int DecoratorCounter;
        internal int InstanceNumber;
        static int InstanceCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilder"/> class.
        /// </summary>
        protected ExpressionBuilder()
        {
            InstanceNumber = Interlocked.Increment(ref InstanceCounter);
        }

        internal ExpressionBuilder(ExpressionBuilder builder, bool decorator)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            InstanceNumber = builder.InstanceNumber;
            DecoratorCounter = builder.DecoratorCounter + (decorator ? 1 : 0);
        }

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
        /// Returns a string that represents the display name of this <see cref="ExpressionBuilder"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="ExpressionBuilder"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{{0}}}", GetElementDisplayName(this));
        }

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

            var disableBuilder = builder as DisableBuilder;
            if (disableBuilder != null) builder = disableBuilder.Builder;

            var combinatorBuilder = builder as CombinatorBuilder;
            if (combinatorBuilder != null) return combinatorBuilder.Combinator;

            return builder;
        }

        public static InspectBuilder GetVisualizerElement(ExpressionBuilder builder)
        {
            var inspectBuilder = (InspectBuilder)builder;
            return GetVisualizerElement(inspectBuilder);
        }

        internal static InspectBuilder GetVisualizerElement(InspectBuilder builder)
        {
            return builder.VisualizerElement ?? builder;
        }

        internal static Type GetWorkflowPropertyType(Type expressionType)
        {
            if (expressionType == typeof(bool)) return typeof(BooleanProperty);
            if (expressionType == typeof(int)) return typeof(IntProperty);
            if (expressionType == typeof(float)) return typeof(FloatProperty);
            if (expressionType == typeof(double)) return typeof(DoubleProperty);
            if (expressionType == typeof(string)) return typeof(StringProperty);
            if (expressionType == typeof(DateTime)) return typeof(DateTimeProperty);
            if (expressionType == typeof(TimeSpan)) return typeof(TimeSpanProperty);
            if (expressionType == typeof(DateTimeOffset)) return typeof(DateTimeOffsetProperty);
            return typeof(WorkflowProperty<>).MakeGenericType(expressionType);
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
                var combinatorAttribute = elementType.GetCustomAttribute<CombinatorAttribute>(true);
                var builderType = Type.GetType(combinatorAttribute.ExpressionBuilderTypeName);
                var builder = (CombinatorBuilder)Activator.CreateInstance(builderType);
                builder.Combinator = element;
                return builder;
            }

            if (elementCategory == ElementCategory.Source)
            {
                return new SourceBuilder { Generator = element };
            }

            throw new InvalidOperationException("Invalid workflow element type.");
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

        #region Member Selector

        internal static Expression MemberSelector(Expression expression, string selector)
        {
            var selectedMembers = ExpressionHelper.SelectMembers(expression, selector).ToArray();
            if (selectedMembers.Length > 1)
            {
                return ExpressionHelper.CreateTuple(selectedMembers);
            }
            else return selectedMembers.Single();
        }

        #endregion

        #region Type Inference

        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        static TSource DefaultIfNotSingle<TSource>(IEnumerable<TSource> source)
        {
            var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext()) return default(TSource);
            var result = enumerator.Current;
            if (enumerator.MoveNext()) return default(TSource);
            return result;
        }

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
                                     let match = DefaultIfNotSingle(matches.Distinct())
                                     where match != null
                                     select match.Item1)
                                     .ToArray();

            return methodGenericArguments.Zip(bindingCandidates, (argument, match) => match).Concat(methodGenericArguments.Skip(bindingCandidates.Length)).ToArray();
        }

        static bool MatchGenericConstraints(Type parameterType, Type argumentType)
        {
            if (!parameterType.IsGenericParameter)
            {
                throw new ArgumentException("The specified parameter is not generic.", "parameterType");
            }

            if (!parameterType.BaseType.IsAssignableFrom(argumentType))
            {
                return false;
            }

            var interfaces = parameterType.GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                if (!interfaceType.IsAssignableFrom(argumentType))
                {
                    return false;
                }
            }

            return true;
        }

        internal static IEnumerable<Tuple<Type, int>> GetParameterBindings(Type parameterType, Type argumentType)
        {
            // If parameter is a generic parameter, just bind it to the input type
            // Any generic constraints on the parameter type must be compatible with the input type
            if (parameterType.IsGenericParameter && MatchGenericConstraints(parameterType, argumentType))
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
                return GetParameterBindings(parameterElementType, argumentElementType);
            }

            // Match bindings can only be obtained if both types are generic types
            if (parameterType.IsGenericType && argumentType.IsGenericType)
            {
                var parameterTypeDefinition = parameterType.GetGenericTypeDefinition();
                var argumentTypeDefinition = argumentType.GetGenericTypeDefinition();
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
                if (argumentTypes[i] != arrayType && !HasObservableConversion(argumentTypes[i], arrayType))
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
                var argument = arguments[k + offset];
                if (argument.Type != arrayType)
                {
                    argument = CoerceMethodArgument(arrayType, argument);
                }
                initializers[k] = argument;
            }

            var paramArray = Expression.NewArrayInit(arrayType, initializers);
            var expandedArguments = new Expression[parameters.Length];
            for (int i = 0; i < expandedArguments.Length - 1; i++)
            {
                expandedArguments[i] = arguments[i];
            }
            expandedArguments[expandedArguments.Length - 1] = paramArray;
            return expandedArguments;
        }

        internal static Expression CoerceMethodArgument(Type parameterType, Expression argument)
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
            else
            {
                return Expression.Convert(argument, parameterType);
            }
        }

        static Expression[] MatchMethodParameters(ParameterInfo[] parameters, Expression[] arguments)
        {
            int i = 0;
            return Array.ConvertAll(arguments, argument =>
            {
                var parameterType = parameters[i++].ParameterType;
                if (argument.Type != parameterType)
                {
                    argument = CoerceMethodArgument(parameterType, argument);
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
            if (nonNullableTo.IsAssignableFrom(nonNullableFrom)) return true;
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

        internal static bool HasConversion(Type from, Type to)
        {
            return HasPrimitiveConversion(from, to) || HasReferenceConversion(from, to) ||
                   GetUserConversion(from, to) != null;
        }

        static bool HasObservableConversion(Type from, Type to)
        {
            if (from.IsGenericType && to.IsGenericType &&
                from.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                to.GetGenericTypeDefinition() == typeof(IObservable<>))
            {
                var argumentObservableType = from.GetGenericArguments()[0];
                var parameterObservableType = to.GetGenericArguments()[0];
                return HasConversion(argumentObservableType, parameterObservableType);
            }

            return HasConversion(from, to);
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
                    return HasObservableConversion(argument.Type, parameterType);
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

        static int CompareSpecialization(Type t1, Type t2, Type s)
        {
            if (t1 == t2) return 0;
            if (s == t1) return -1;
            if (s == t2) return 1;

            // Check if t2 is reducible to t1 and vice-versa:
            //   - if they are both reducible to each other or if neither is reducible, we have a tie
            //   - if only t2 is reducible to t1, then take t2 (it is more specialized); conversely for t1
            var bindingT2T1 = GetParameterBindings(t1, t2).Any();
            var bindingT1T2 = GetParameterBindings(t2, t1).Any();
            if (bindingT2T1 && bindingT1T2) return 0;
            else if (bindingT2T1) return 1;
            else if (bindingT1T2) return -1;
            else return 0;
        }

        static int CompareFunctionMember(Type[] parametersA, Type[] parametersB, Type[] arguments, Func<Type, Type, Type, int> comparison)
        {
            bool? betterA = null;
            bool? betterB = null;
            for (int i = 0; i < arguments.Length; i++)
            {
                var result = comparison(parametersA[i], parametersB[i], arguments[i]);
                if (result < 0)
                {
                    if (!betterA.HasValue) betterA = true;
                    betterB = false;
                }
                else if (result > 0)
                {
                    if (!betterB.HasValue) betterB = true;
                    betterA = false;
                }
            }

            if (betterA.GetValueOrDefault()) return -1;
            if (betterB.GetValueOrDefault()) return 1;
            return 0;
        }

        static bool IsObservable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IObservable<>);
        }

        static Type GetObservableElementType(Type type)
        {
            return IsObservable(type) ? type.GetGenericArguments()[0] : type;
        }

        static Type[] ExpandCallParameterTypes(ParameterInfo[] parameters, Type[] arguments, bool expansion)
        {
            var parameterCount = expansion ? parameters.Length - 1 : parameters.Length;
            var expandedParameters = new Type[arguments.Length];
            for (int i = 0; i < parameterCount; i++)
            {
                expandedParameters[i] = GetObservableElementType(parameters[i].ParameterType);
            }

            if (expansion)
            {
                for (int i = parameters.Length - 1; i < expandedParameters.Length; i++)
                {
                    expandedParameters[i] = GetObservableElementType(parameters[parameters.Length - 1].ParameterType.GetElementType());
                }
            }

            return expandedParameters;
        }

        class CallCandidate
        {
            internal static readonly CallCandidate Ambiguous = new CallCandidate();
            internal static readonly CallCandidate None = new CallCandidate();
            internal MethodBase method;
            internal Expression[] arguments;
            internal bool generic;
            internal bool expansion;
            internal bool excluded;
        }

        internal static Expression BuildCall(Expression instance, IEnumerable<MethodInfo> methods, params Expression[] arguments)
        {
            var overload = OverloadResolution(methods, arguments);
            if (overload == CallCandidate.None) throw new InvalidOperationException("No method overload found for the given arguments.");
            if (overload == CallCandidate.Ambiguous) throw new InvalidOperationException("The method overload call is ambiguous.");
            return Expression.Call(instance, (MethodInfo)overload.method, overload.arguments);
        }

        static CallCandidate OverloadResolution(IEnumerable<MethodBase> methods, params Expression[] arguments)
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
                    Expression[] callArguments;
                    ParameterInfo[] parameters;
                    try
                    {
                        if (method.IsGenericMethodDefinition)
                        {
                            method = MakeGenericMethod((MethodInfo)method, argumentTypes);
                            if (method.IsGenericMethodDefinition) return null;
                        }

                        callArguments = arguments;
                        parameters = method.GetParameters();
                        if (ParamExpansionRequired(parameters, argumentTypes))
                        {
                            if (!CanExpandParamArguments(parameters, argumentTypes)) return null;
                            callArguments = ExpandParamArguments(parameters, callArguments);
                        }

                        if (!CanMatchMethodParameters(parameters, callArguments)) return null;
                        callArguments = MatchMethodParameters(parameters, callArguments);
                    }
                    catch (ArgumentException) { return null; }
                    catch (InvalidOperationException) { return null; }
                    return new CallCandidate
                    {
                        method = method,
                        arguments = callArguments,
                        generic = method.IsGenericMethod,
                        expansion = ParamExpansionRequired(parameters, argumentTypes),
                        excluded = false
                    };
                })
                .Where(candidate => candidate != null)
                .ToArray();

            if (candidates.Length == 0) return CallCandidate.None;
            if (candidates.Length == 1) return candidates[0];

            argumentTypes = Array.ConvertAll(argumentTypes, argumentType => GetObservableElementType(argumentType));
            var genericParameters = Array.ConvertAll(
                candidates,
                candidate => ExpandCallParameterTypes(
                    (candidate.method.IsGenericMethod
                    ? ((MethodInfo)candidate.method).GetGenericMethodDefinition()
                    : candidate.method).GetParameters(),
                    argumentTypes,
                    candidate.expansion));
            var candidateParameters = Array.ConvertAll(
                candidates,
                candidate => ExpandCallParameterTypes(candidate.method.GetParameters(), argumentTypes, candidate.expansion));

            for (int i = 0; i < candidateParameters.Length; i++)
            {
                // skip excluded candidates
                if (candidates[i].excluded) continue;

                for (int j = 0; j < candidateParameters.Length; j++)
                {
                    // skip self-test
                    if (i == j) continue;

                    // compare implicit type conversion
                    var comparison = CompareFunctionMember(
                        candidateParameters[i],
                        candidateParameters[j],
                        argumentTypes,
                        CompareConversion);
                    if (comparison == 0) // tie-break
                    {
                        // non-generic vs generic
                        if (!candidates[i].generic && candidates[j].generic) comparison = -1;
                        else if (!candidates[j].generic && candidates[i].generic) comparison = 1;
                        // non-params vs params
                        else if (!candidates[i].expansion && candidates[j].expansion) comparison = -1;
                        else if (!candidates[j].expansion && candidates[i].expansion) comparison = 1;
                        else
                        {   // compare parameter specialization
                            comparison = CompareFunctionMember(
                                genericParameters[i],
                                genericParameters[j],
                                argumentTypes,
                                CompareSpecialization);
                        }
                    }

                    // exclude self if loss or tied; exclude other if win or tied
                    if (comparison >= 0) candidates[i].excluded = true;
                    if (comparison <= 0) candidates[j].excluded = true;
                }

                // return the single survivor
                if (!candidates[i].excluded) return candidates[i];
            }

            return CallCandidate.Ambiguous;
        }

        #endregion

        #region Nested Workflow Output

        internal static bool IsReducible(Expression expression)
        {
            return expression.NodeType != ExpressionType.Extension || expression.CanReduce;
        }

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
            return MergeDependencies(source, Observable.Merge(connections).Select(xs => default(TSource)));
        }

        internal static Expression BuildOutput(Expression output, IEnumerable<Expression> connections)
        {
            var ignoredConnections = (from connection in connections.Where(IsReducible)
                                      let observableType = connection.Type.GetGenericArguments()[0]
                                      select Expression.Call(typeof(ExpressionBuilder), "IgnoreConnection", new[] { observableType }, connection))
                                      .ToArray();
            if (output != null && ignoredConnections.Length == 0)
            {
                return output;
            }

            var connectionArrayExpression = Expression.NewArrayInit(typeof(IObservable<Unit>), ignoredConnections);
            if (output != null)
            {
                var outputType = output.Type.GetGenericArguments()[0];
                return Expression.Call(typeof(ExpressionBuilder), "MergeOutput", new[] { outputType }, output, connectionArrayExpression);
            }
            else return Expression.Call(typeof(ExpressionBuilder), "MergeOutput", null, connectionArrayExpression);
        }

        #endregion

        #region Error Handling

        static readonly MethodInfo throwMethod = typeof(Observable).GetMethods()
                                                                   .Where(m => m.Name == "Throw")
                                                                   .Single(m => m.GetParameters().Length == 1);

        internal static Expression HandleObservableCreationException(Expression expression)
        {
            var exceptionVariable = Expression.Variable(typeof(Exception));
            var observableType = expression.Type.GetGenericArguments()[0];
            return Expression.TryCatch(
                expression,
                Expression.Catch(
                    exceptionVariable,
                    Expression.Call(
                        throwMethod.MakeGenericMethod(observableType),
                        exceptionVariable)));
        }

        #endregion

        #region Dynamic Properties

        static readonly MethodInfo deferMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Defer" &&
                                                                                m.GetParameters()[0].ParameterType
                                                                                 .GetGenericArguments()[0]
                                                                                 .GetGenericTypeDefinition() == typeof(IObservable<>));

        internal static Tuple<Expression, string> BuildArgumentAccess(IEnumerable<Expression> arguments, string selector)
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

        [Obsolete]
        protected static IEnumerable<Expression> SelectMembers(Expression expression, string selector)
        {
            return ExpressionHelper.SelectMembers(expression, selector);
        }

        [Obsolete]
        protected Tuple<Expression, string> GetArgumentAccess(IEnumerable<Expression> arguments, string selector)
        {
            return BuildArgumentAccess(arguments, selector);
        }

        internal static Expression BuildPropertyMapping(Expression source, ConstantExpression instance, string propertyName)
        {
            return BuildPropertyMapping(source, instance, propertyName, string.Empty);
        }

        internal static Expression BuildPropertyMapping(Expression source, ConstantExpression instance, string propertyName, string sourceSelector)
        {
            var element = instance.Value;
            var workflowBuilder = element as IWorkflowExpressionBuilder;
            if (workflowBuilder != null && workflowBuilder.Workflow != null)
            {
                var inputBuilder = (from node in workflowBuilder.Workflow
                                    let externalizedBuilder = Unwrap(node.Value) as IExternalizedMappingBuilder
                                    where externalizedBuilder != null
                                    from workflowProperty in externalizedBuilder.GetExternalizedProperties()
                                    where workflowProperty.ExternalizedName == propertyName
                                    select new { node, workflowProperty }).FirstOrDefault();
                if (inputBuilder == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "The specified property '{0}' was not found in the nested workflow.",
                        propertyName));
                }

                // Checking nested externalized properties requires only one level of indirection
                if (source == EmptyExpression.Instance) return source;

                var argument = source;
                foreach (var successor in inputBuilder.node.Successors)
                {
                    var successorElement = ExpressionBuilder.GetWorkflowElement(successor.Target.Value);
                    var successorInstance = Expression.Constant(successorElement);
                    argument = BuildPropertyMapping(argument, successorInstance, inputBuilder.workflowProperty.Name, sourceSelector);
                }
                return argument;
            }

            //TODO: The special case for binary operator operands should be avoided in the future
            var binaryOperator = element as BinaryOperatorBuilder;
            if (binaryOperator != null && binaryOperator.Operand != null)
            {
                instance = Expression.Constant(binaryOperator.Operand);
            }

            var property = Expression.Property(instance, propertyName);
            if (source == EmptyExpression.Instance) return source;
            var sourceType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(sourceType);
            var body = BuildTypeMapping(parameter, property.Type, sourceSelector);
            body = Expression.Assign(property, body);

            var actionType = Expression.GetActionType(parameter.Type);
            var action = Expression.Lambda(actionType, body, parameter);
            return Expression.Call(
                typeof(ExpressionBuilder),
                "PropertyMapping",
                new[] { sourceType },
                source,
                action);
        }

        internal static Expression BuildTypeMapping(Expression expression, Type targetType, string selector)
        {
            var result = expression;
            if (!string.IsNullOrEmpty(selector))
            {
                result = MemberSelector(result, selector);
            }

            if (targetType != null && result.Type != targetType)
            {
                if (HasConversion(result.Type, targetType))
                {
                    result = Expression.Convert(result, targetType);
                }
                else
                {
                    var arguments = ExpressionHelper.SelectMembers(expression, selector).ToArray();
                    var propertyType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    var constructor = OverloadResolution(propertyType.GetConstructors(), arguments);
                    if (constructor.method != null)
                    {
                        result = Expression.New((ConstructorInfo)constructor.method, constructor.arguments);
                        if (propertyType != targetType)
                        {
                            result = Expression.Convert(result, targetType);
                        }
                    }
                }
            }

            return result;
        }

        static IObservable<TSource> PropertyMapping<TSource>(IObservable<TSource> source, Action<TSource> action)
        {
            return source.Do(action);
        }

        static IObservable<TResult> PropertyMapping<TSource, TResult>(IObservable<TSource> source, Action<TSource> action)
        {
            return source.Do(action).IgnoreElements().Select(xs => default(TResult));
        }

        #endregion

        #region Build Dependencies

        internal static bool IsBuildDependency(IArgumentBuilder argumentBuilder)
        {
            return argumentBuilder != null && !(argumentBuilder is InputMappingBuilder);
        }

        static Expression BuildDependency(Expression source, Expression output)
        {
            var sourceType = source.Type.GetGenericArguments()[0];
            var outputType = output.Type.GetGenericArguments()[0];
            return Expression.Call(
                typeof(ExpressionBuilder),
                "BuildDependency",
                new[] { sourceType, outputType },
                source);
        }

        static IObservable<TResult> BuildDependency<TSource, TResult>(IObservable<TSource> source)
        {
            return source.IgnoreElements().Select(xs => default(TResult));
        }

        internal static Expression MergeBuildDependencies(Expression output, IEnumerable<Expression> buildDependencies)
        {
            var observableFactory = Expression.Lambda(output);
            var outputType = output.Type.GetGenericArguments()[0];
            var source = Expression.Call(deferMethod.MakeGenericMethod(outputType), observableFactory);
            buildDependencies = buildDependencies.Select(dependency => BuildDependency(dependency, output));
            var mappingArray = Expression.NewArrayInit(output.Type, buildDependencies);
            return Expression.Call(
                typeof(ExpressionBuilder),
                "MergeDependencies",
                new[] { outputType },
                source,
                mappingArray);
        }

        internal static IObservable<TSource> MergeDependencies<TSource>(IObservable<TSource> source, params IObservable<TSource>[] dependencies)
        {
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }

            if (dependencies.Length == 0)
            {
                return source;
            }

            return Observable.Create<TSource>(observer =>
            {
                var dependencyDisposable = new SingleAssignmentDisposable();
                var dependencyObservable = dependencies.Length == 1 ? dependencies[0] : dependencies.Merge(Scheduler.Immediate);
                dependencyDisposable.Disposable = dependencyObservable.Subscribe(
                    value => { },
                    error =>
                    {
                        using (dependencyDisposable)
                            observer.OnError(error);
                    },
                    () => { });

                if (dependencyDisposable.IsDisposed) return dependencyDisposable;
                else return new CompositeDisposable(dependencyDisposable, source.SubscribeSafe(observer));
            });
        }

        #endregion
    }
}
