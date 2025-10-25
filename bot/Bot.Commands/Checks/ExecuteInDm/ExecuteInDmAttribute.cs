using DSharpPlus.Commands.ContextChecks;

namespace Bot.Commands.Checks.ExecuteInDm;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class ExecuteInDmAttribute : ContextCheckAttribute
{
    public bool Allowed { get; }
    public bool OnlyInDm { get; }

    public ExecuteInDmAttribute(bool allowed = true, bool onlyInDm = false)
    {
        Allowed = allowed;
        OnlyInDm = onlyInDm;
    }
}