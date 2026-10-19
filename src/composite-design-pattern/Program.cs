using CompositeDesignPattern;

var flatRows = new List<EmployeeHierarchyMap>
{
    new() { ManagerRefId = 1, SubordinateRefId = 2, Depth = 1 },
    new() { ManagerRefId = 1, SubordinateRefId = 3, Depth = 1 },
    new() { ManagerRefId = 2, SubordinateRefId = 4, Depth = 1 },
    new() { ManagerRefId = 2, SubordinateRefId = 5, Depth = 1 },
    new() { ManagerRefId = 3, SubordinateRefId = 6, Depth = 1 },
    new() { ManagerRefId = 1, SubordinateRefId = 4, Depth = 2 },
    new() { ManagerRefId = 1, SubordinateRefId = 5, Depth = 2 },
    new() { ManagerRefId = 1, SubordinateRefId = 6, Depth = 2 },
};

var builder = new HierarchyBuilder(flatRows);
var tree = builder.BuildTree(rootManagerId: 1);

Console.WriteLine($"CEO total headcount: {tree.TotalDescendantCount}");

var found = tree.Find(employeeId: 5);
Console.WriteLine($"Found employee 5: {found?.Name ?? "not found"}");

var allManagers = tree.Filter(n => n is ManagerNode).Cast<ManagerNode>();
Console.WriteLine($"Managers in tree: {string.Join(", ", allManagers.Select(m => m.Name))}");
