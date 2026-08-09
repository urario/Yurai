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
        Assert.Equal(4, methods.Length);
        Assert.Equal(2, methods.Count(method => method.Name == "Of"));
        Assert.Equal(1, methods.Count(method => method.Name == "Min"));
        Assert.Equal(1, methods.Count(method => method.Name == "Max"));
        Assert.Contains(methods, method => HasParameters(method, typeof(decimal)));
        Assert.Contains(methods, method => HasParameters(method, typeof(decimal), typeof(string)));
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
                "As", "Round", "ToString", "get_Value",
                "op_Addition", "op_Addition", "op_Addition",
                "op_Division", "op_Division", "op_Division",
                "op_Multiply", "op_Multiply", "op_Multiply",
                "op_Subtraction", "op_Subtraction", "op_Subtraction",
            ],
            methodNames);
        Assert.Equal(typeof(decimal), carrier.GetProperty("Value")?.PropertyType);
        Assert.Null(carrier.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static));
        Assert.Null(carrier.GetMethod("op_Equality", BindingFlags.Public | BindingFlags.Static));
        Assert.Null(carrier.GetMethod("op_Inequality", BindingFlags.Public | BindingFlags.Static));
    }

    private static bool HasParameters(MethodInfo method, params Type[] parameterTypes) =>
        method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes);
}
