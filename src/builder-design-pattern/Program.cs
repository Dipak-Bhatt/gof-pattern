using BuilderDesignPattern;

var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var director = new AuditEventDirector(new ConsoleAuditLogger());

Console.WriteLine("=== Builder Pattern — audit events ===\n");
Console.WriteLine("DIRECTOR  AuditEventDirector.Add() / Update() / ...");
Console.WriteLine("BUILDER   AuditEventBuilder.Set...() then Build() or Publish()");
Console.WriteLine("PRODUCT   AuditEvent (immutable)\n");

Console.WriteLine("--- Add: target id and metadata ---");
director.Add()
    .SetCategory(AuditCategory.Department)
    .SetTarget(12)
    .SetMetadata(new { Name = "Engineering", Code = "ENG" })
    .SetTenant(tenantId)
    .Publish();

Console.WriteLine("\n--- Update: before/after objects become JSON ---");
director.Update()
    .SetCategory(AuditCategory.Employee)
    .SetTarget(42, "Ram Bahadur")
    .SetEmployee(7)
    .SetMessage("Department changed")
    .SetBefore(new { Name = "Ram Bahadur", DepartmentId = 7 })
    .SetAfter(new { Name = "Ram Bahadur", DepartmentId = 12 })
    .Publish();

Console.WriteLine("\n--- Update with no real change: state is omitted ---");
director.Update()
    .SetCategory(AuditCategory.Employee)
    .SetTarget(42, "Ram Bahadur")
    .SetBefore(new { DepartmentId = 12 })
    .SetAfter(new { DepartmentId = 12 })
    .Publish();

Console.WriteLine("\n--- Upload: target name defaults from the category ---");
director.Upload()
    .SetCategory(AuditCategory.Payslip)
    .SetTarget("payslip-2026-10.pdf")
    .Publish();

Console.WriteLine("\n--- Delete ---");
director.Delete()
    .SetCategory(AuditCategory.Role)
    .SetTarget(99, "Payroll Admin")
    .Publish();

Console.WriteLine("\n--- Validation: forgetting the category fails at Build ---");
try
{
    director.Record()
        .SetAction(LogAction.Accessed)
        .Publish();
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  rejected: {ex.Message}");
}

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("The builder assembles and validates; the immutable event is the product.");
Console.WriteLine("Each call site chains only the parts it has, and Publish() hands it to the receiver.");
