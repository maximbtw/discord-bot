using DSharpPlus.Commands.ContextChecks;

namespace Bot.Commands.Checks.RequireApplicationOwner;

// TODO: RequireApplicationOwner из библиотеки по какой-то причине не работает корректно, в бп не попадаю.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class MyRequireApplicationOwnerAttribute : ContextCheckAttribute;
