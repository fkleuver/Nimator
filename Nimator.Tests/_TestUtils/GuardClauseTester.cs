using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture.Kernel;
using FluentAssertions;

namespace Nimator.Tests
{
    public static class GuardClauseTester
    {
        /// <summary>
        /// Scans the provided type for any constructors  that have parameter annotations
        /// from Nimator\Properties\ReSharper.Annotations.g.cs and verifies that each individual parameter, if given null 
        /// (or empty), throws the expected exception (or doesn't, if the inverse attribute type is applied).
        /// </summary>
        /// <returns>How many ArgumentExceptions were thrown (should be one per guard clause)</returns>
        public static int VerifyConstructorGuards(this Type type, ISpecimenContext ctx = null)
        {
            var exceptions = 0;
            foreach (var ctor in type.GetConstructors())
            {
                exceptions += VerifyGuards(ctor, ctx);
            }
            return exceptions;
        }

        /// <summary>
        /// Scans the provided type for any static methods  that have parameter annotations
        /// from Nimator\Properties\ReSharper.Annotations.g.cs and verifies that each individual parameter, if given null 
        /// (or empty), throws the expected exception (or doesn't, if the inverse attribute type is applied).
        /// </summary>
        /// <returns>How many ArgumentExceptions were thrown (should be one per guard clause)</returns>
        public static int VerifyStaticMethodGuards(this Type type, ISpecimenContext ctx = null)
        {
            var exceptions = 0;
            foreach (var ctor in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                exceptions += VerifyGuards(ctor, ctx);
            }
            return exceptions;
        }

        /// <summary>
        /// Scans the provided type for any instance methods that have parameter annotations
        /// from Nimator\Properties\ReSharper.Annotations.g.cs and verifies that each individual parameter, if given null 
        /// (or empty), throws the expected exception (or doesn't, if the inverse attribute type is applied).
        /// </summary>
        /// <returns>How many ArgumentExceptions were thrown (should be one per guard clause)</returns>
        public static int VerifyInstanceMethodGuards(this Type type, object instance, ISpecimenContext ctx = null)
        {
            var exceptions = 0;
            foreach (var ctor in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                exceptions += VerifyGuards(ctor, ctx, instance);
            }
            return exceptions;
        }

        private static int VerifyGuards(MethodBase methodBase, ISpecimenContext ctx = null, object instance = null)
        {
            ctx = ctx ?? DefaultFixture.CreateContext();
            var method = methodBase as MethodInfo;
            var ctor = methodBase as ConstructorInfo;
            if (method != null && method.IsGenericMethod)
            {
                var typeArgs = method.GetGenericArguments().Select(a => typeof(string)).ToArray();
                method = method.MakeGenericMethod(typeArgs);
            }
            var methodParameters = ((MethodBase)method ?? ctor).GetParameters();
            var parameterCount = methodParameters.Length;
            var signatureForDisplay = string.Join(", ", methodParameters.Select(p => p.ParameterType.Name));
            var exceptionCount = 0;

            for (var annotatedParamIdx = 0; annotatedParamIdx < parameterCount; annotatedParamIdx++)
            {
                var annotatedParam = methodParameters[annotatedParamIdx];
                var annotationAttributes = annotatedParam.GetCustomAttributes().Where(IsReSharperAnnotationAttributeType);
                foreach (var annotation in annotationAttributes)
                {
                    var paramList = new object[parameterCount];
                    for (var currentParamIdx = 0; currentParamIdx < parameterCount; currentParamIdx++)
                    {
                        var currentParam = methodParameters[currentParamIdx];
                        var currentParamType = currentParam.ParameterType;
                        if (currentParamIdx != annotatedParamIdx)
                        {
                            paramList[currentParamIdx] = ctx.Resolve(currentParamType);
                        }
                        else
                        {
                            // The null data to make the guard clause fail is created by GuardClauseFailingDataBuilder
                            paramList[currentParamIdx] = ctx.Resolve(new Tuple<Type, Attribute>(currentParamType, annotation));
                        }
                    }

                    Exception ex = null;
                    try
                    {
                        if (method != null)
                        {
                            method.Invoke(instance, paramList);
                        }
                        else if (ctor != null)
                        {
                            ctor.Invoke(paramList);
                        }
                    }
                    catch (TargetInvocationException e)
                    {
                        ex = e.InnerException;
                    }

                    switch (annotation)
                    {
                        case CanBeNullAttribute _:
                        case ItemCanBeNullAttribute _:
                        case CanBeEmptyAttribute _:
                            {
                                ex?.Should().NotBeAssignableTo<ArgumentException>($"parameter {annotatedParamIdx} in signature ({signatureForDisplay}) should allow null");
                                break;
                            }
                        case NotNullAttribute _:
                            {
                                ex.Should().BeOfType<ArgumentNullException>($"parameter {annotatedParamIdx} in signature ({signatureForDisplay}) should not allow null");
                                exceptionCount++;
                                break;
                            }
                        case ItemNotNullAttribute _:
                            {
                                ex.Should().BeOfType<ArgumentNullException>($"parameter {annotatedParamIdx} in signature ({signatureForDisplay}) should not allow null items");
                                exceptionCount++;
                                break;
                            }
                        case NotEmptyAttribute _:
                            {
                                ex.Should().BeOfType<ArgumentOutOfRangeException>($"parameter {annotatedParamIdx} in signature ({signatureForDisplay}) should not allow empty collections");
                                exceptionCount++;
                                break;
                            }
                    }
                }
            }
            return exceptionCount;
        }

        private static bool IsReSharperAnnotationAttributeType(Attribute attribute)
        {
            return ReSharperAnnotationAttributeTypes.Contains(attribute.GetType());
        }

        private static IEnumerable<Type> ReSharperAnnotationAttributeTypes { get; } = new[]
        {
            typeof(CanBeNullAttribute),
            typeof(ItemCanBeNullAttribute),
            typeof(NotNullAttribute),
            typeof(ItemNotNullAttribute),
            typeof(NotEmptyAttribute),
            typeof(CanBeEmptyAttribute)
        };
    }
}

