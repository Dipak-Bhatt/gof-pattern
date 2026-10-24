namespace IteratorDesignPattern.Models;

/// <summary>A tenant a scheduled run may visit.</summary>
/// <param name="Id">Stable identity; the run's order is sorted by it.</param>
/// <param name="Name">Shown in the console.</param>
public sealed record Tenant(Guid Id, string Name);
