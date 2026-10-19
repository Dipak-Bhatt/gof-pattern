namespace CompositeDesignPattern;

public interface IOrgNode
{
    int EmployeeId { get; }
    string Name { get; }
    IReadOnlyList<IOrgNode> DirectReports { get; }
    int TotalDescendantCount { get; }

    IOrgNode? Find(int employeeId);
    IEnumerable<IOrgNode> Filter(Func<IOrgNode, bool> predicate);
}

public sealed class EmployeeLeaf : IOrgNode
{
    public required int EmployeeId { get; init; }
    public required string Name { get; init; }
    public IReadOnlyList<IOrgNode> DirectReports => [];
    public int TotalDescendantCount => 0;

    public IOrgNode? Find(int employeeId) =>
        EmployeeId == employeeId ? this : null;

    public IEnumerable<IOrgNode> Filter(Func<IOrgNode, bool> predicate) =>
        predicate(this) ? [this] : [];
}

public sealed class ManagerNode : IOrgNode
{
    public required int EmployeeId { get; init; }
    public required string Name { get; init; }
    public required List<IOrgNode> DirectReports { get; init; }
    IReadOnlyList<IOrgNode> IOrgNode.DirectReports => DirectReports;

    public int TotalDescendantCount =>
        DirectReports.Sum(r => 1 + r.TotalDescendantCount);

    public IOrgNode? Find(int employeeId)
    {
        if (EmployeeId == employeeId) return this;
        return DirectReports
            .Select(r => r.Find(employeeId))
            .FirstOrDefault(r => r is not null);
    }

    public IEnumerable<IOrgNode> Filter(Func<IOrgNode, bool> predicate)
    {
        if (predicate(this)) yield return this;
        foreach (var child in DirectReports)
            foreach (var match in child.Filter(predicate))
                yield return match;
    }
}

public class EmployeeHierarchyMap
{
    public int ManagerRefId { get; set; }
    public int SubordinateRefId { get; set; }
    public int Depth { get; set; }
}
