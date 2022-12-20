namespace Dolite.Domain.Contracts.CodeGenerator;

[AttributeUsage(AttributeTargets.Class)]
public class ApiServiceAttribute : Attribute
{
    public string? Tag { get; init; }
    public string Rule { get; init; } = ".*";
}