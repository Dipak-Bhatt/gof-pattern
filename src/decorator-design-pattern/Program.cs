using DecoratorDesignPattern;

// ─────────────────────────────────────────────────────────────────────────────
// Scenario 1 — a read path, decorated with caching and tenant auditing.
// ─────────────────────────────────────────────────────────────────────────────

var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

Console.WriteLine("=== Decorator Pattern — Scenario 1: cross-cutting read concerns ===\n");
Console.WriteLine("The client holds ONE interface, IEmployeeReader. It cannot tell a plain reader from");
Console.WriteLine("a stack of decorators — that is the point of the pattern.\n");

IEmployeeReader plain = new EmployeeReader();

Console.WriteLine("--- Plain component ---");
Console.WriteLine($"  {await plain.ReadAsync(42)}");
Console.WriteLine($"  {await plain.ReadAsync(42)}   // read again: the database is hit twice\n");

Console.WriteLine("--- Wrapped with caching (outer) then auditing (inner) ---");
IEmployeeReader cached = new CachingEmployeeReaderDecorator(plain);
IEmployeeReader decorated = new AuditingEmployeeReaderDecorator(tenantId, cached);

Console.WriteLine($"  {await decorated.ReadAsync(42)}");
Console.WriteLine($"  {await decorated.ReadAsync(42)}   // read again: audited, served from cache");
Console.WriteLine($"  {await decorated.ReadAsync(99) as object ?? "not found"}");

Console.WriteLine("\n--- Same stack, another tenant ---");
IEmployeeReader otherTenant = new AuditingEmployeeReaderDecorator(
    Guid.Parse("99999999-9999-9999-9999-999999999999"), cached);
Console.WriteLine($"  {await otherTenant.ReadAsync(42)}\n");

// ─────────────────────────────────────────────────────────────────────────────
// Scenario 2 — a send path, decorated with retry, audit and metrics.
// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Decorator Pattern — Scenario 2: notify send path ===\n");
Console.WriteLine("Same shape, different domain and different behaviours. The caller still holds ONE");
Console.WriteLine("interface, INotificationSender; the decorators stack in any order.\n");

var payslip = new NotificationIntent("hr.payroll.payslip", "payslip-2026-10-19");

Console.WriteLine("--- Stack A: Metrics( Audit( Retry( outbox ) ) ) — retry closest to the outbox ---");
var outboxA = new OutboxNotificationSender();
var stackA = new MetricsNotificationSenderDecorator(
    new AuditNotificationSenderDecorator(
        new RetryingNotificationSenderDecorator(outboxA)));
Console.WriteLine($"  result: {await stackA.SendAsync(payslip)}   // transient failure recovered in-process");
Console.WriteLine($"  metrics: sent={stackA.Sent} refused={stackA.Refused}\n");

Console.WriteLine("--- Stack B: Retry( Audit( Metrics( outbox ) ) ) — metrics closest to the outbox ---");
var outboxB = new OutboxNotificationSender();
var stackB = new RetryingNotificationSenderDecorator(
    new AuditNotificationSenderDecorator(
        new MetricsNotificationSenderDecorator(outboxB)));
Console.WriteLine($"  result: {await stackB.SendAsync(payslip)}");
var metricsB = Unwrap(stackB);
Console.WriteLine($"  metrics: sent={metricsB.Sent} refused={metricsB.Refused}   // counts the retry's attempts\n");

// ─────────────────────────────────────────────────────────────────────────────

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("One component, many optional behaviours, composed at the call site. Each decorator");
Console.WriteLine("implements the same interface and forwards to the one it wraps, so they stack in any");
Console.WriteLine("order and the client never knows. In Scenario 2 the order matters: metrics on the");
Console.WriteLine("outside counts the outcome once; metrics on the inside counts every retry attempt.");
Console.WriteLine("Real home: an IRepository<T> or IHrNotifications wrapped with caching, audit, retry or");
Console.WriteLine("metrics without editing the component itself.");

// Walks in to the metrics decorator wherever it sits in the stack.
static MetricsNotificationSenderDecorator Unwrap(INotificationSender sender) => sender switch
{
    MetricsNotificationSenderDecorator m => m,
    NotificationSenderDecorator d => Unwrap(d.Inner),
    _ => throw new InvalidOperationException("no metrics decorator in this stack")
};
