namespace CompositeDesignPattern;

public class HierarchyBuilder
{
    private readonly List<EmployeeHierarchyMap> _flat;
    private readonly Dictionary<int, string> _names = new()
    {
        [1] = "Alice (CEO)",
        [2] = "Bob (VP Eng)",
        [3] = "Carol (VP Sales)",
        [4] = "Dev1",
        [5] = "Dev2",
        [6] = "Sales1",
    };

    public HierarchyBuilder(List<EmployeeHierarchyMap> flat) => _flat = flat;

    public IOrgNode BuildTree(int rootManagerId)
    {
        var childIds = _flat
            .Where(h => h.ManagerRefId == rootManagerId && h.Depth == 1)
            .Select(h => h.SubordinateRefId)
            .ToList();

        var children = childIds
            .Select(id => BuildTree(id))
            .ToList();

        if (children.Count == 0)
            return new EmployeeLeaf
            {
                EmployeeId = rootManagerId,
                Name = _names.GetValueOrDefault(rootManagerId, $"Emp{rootManagerId}")
            };

        return new ManagerNode
        {
            EmployeeId = rootManagerId,
            Name = _names.GetValueOrDefault(rootManagerId, $"Emp{rootManagerId}"),
            DirectReports = children
        };
    }
}
