using System.Reflection;
using Xunit;
using TracedValue = global::Yurai.Traced;
using YuraiApi = global::Yurai.Yurai;

namespace Yurai.Tests;

public sealed class PublicApiTests
{
    [Fact]
    public void AssemblyExportsOnlyTheApprovedCoreTypes()
    {
        Type[] exportedTypes = typeof(TracedValue).Assembly.ExportedTypes
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([typeof(TracedValue), typeof(YuraiApi)], exportedTypes);
    }

    [Fact]
    public void FacadeContainsOnlyTheApprovedCreationOverloads()
    {
        Type facade = typeof(YuraiApi);
        MethodInfo[] methods = facade.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.True(facade.IsAbstract && facade.IsSealed);
        Assert.Empty(facade.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(5, methods.Length);
        Assert.Equal(2, methods.Count(method => method.Name == "Of"));
        Assert.Equal(1, methods.Count(method => method.Name == "Min"));
        Assert.Equal(1, methods.Count(method => method.Name == "Max"));
        Assert.Equal(1, methods.Count(method => method.Name == "If"));
        Assert.Contains(methods, method => HasParameters(method, typeof(decimal)));
        Assert.Contains(methods, method => HasParameters(method, typeof(decimal), typeof(string)));
        MethodInfo conditional = Assert.Single(methods, method => method.Name == "If");
        Assert.True(HasParameters(
            conditional,
            typeof(bool),
            typeof(Func<TracedValue>),
            typeof(Func<TracedValue>),
            typeof(string)));
        Assert.Equal(
            ["condition", "whenTrue", "whenFalse", "branchName"],
            conditional.GetParameters().Select(parameter => parameter.Name));
        Assert.All(methods, method => Assert.Equal(typeof(TracedValue), method.ReturnType));
    }

    [Fact]
    public void CarrierContainsOnlyTheApprovedDeclaredMembers()
    {
        Type carrier = typeof(TracedValue);
        string[] methodNames = carrier
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.True(carrier.IsValueType);
        Assert.True(carrier.IsSealed);
        Assert.Empty(carrier.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(
            [
                "As", "DependsOn", "Explain", "Round", "ToJson", "ToString", "Trace", "get_Inputs", "get_Value",
                "op_Addition", "op_Addition", "op_Addition",
                "op_Division", "op_Division", "op_Division",
                "op_Multiply", "op_Multiply", "op_Multiply",
                "op_Subtraction", "op_Subtraction", "op_Subtraction",
            ],
            methodNames);
        Assert.Equal(typeof(decimal), carrier.GetProperty("Value")?.PropertyType);
        Assert.Equal(typeof(IReadOnlyList<string>), carrier.GetProperty("Inputs")?.PropertyType);
        Assert.Equal(typeof(bool), carrier.GetMethod("DependsOn", [typeof(string)])?.ReturnType);
        Assert.Equal(
            typeof(IReadOnlyList<IReadOnlyList<string>>),
            carrier.GetMethod("Trace", [typeof(string)])?.ReturnType);
        Assert.NotNull(carrier.GetMethod("Round", [typeof(int), typeof(string)]));
        Assert.Equal(typeof(string), carrier.GetMethod("ToJson", Type.EmptyTypes)?.ReturnType);
        Assert.Null(carrier.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static));
        Assert.Null(carrier.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static));
        Assert.Null(carrier.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static));
    }

    private static bool HasParameters(MethodInfo method, params Type[] parameterTypes) =>
        method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes);
}
