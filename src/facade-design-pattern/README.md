# Facade Design Pattern

## Definition

The Facade pattern **gives a set of related operations a single, simple entry point**. The caller makes one call — for example `CompleteExitAsync(employeeId, subscriptionId)` — and the facade decides which subsystems to call, in what order, and whether the request is even eligible. The subsystems still exist and can be used directly; the facade only hides the common case's sequence and rules.

A facade adds **no new behaviour**. It composes behaviour that already lives in the subsystems, so the caller never has to know that the account, the exit record and the reporting line must change together.

## Structure

```
┌─────────────────────┐
│   Client            │
│   Program.cs        │
└──────────┬──────────┘
           │  one call: CompleteExitAsync(id, subscriptionId)
           ▼
┌─────────────────────────────────────┐
│   FACADE                            │
│   OffboardingFacade                 │
│   - checks eligibility              │
│   - decides the order of operations │
│   - translates the result           │
└───────┬──────────┬──────────┬───────┘
        │          │          │
        ▼          ▼          ▼
 IPersonnelService  IExitRecordsService  IAccountService  IReportingLineService
 (exit intent)      (exit dates)         (close account)  (reporting line)
```

## Project Layout

```
facade-design-pattern/
├── Employee.cs                     — employee entity and in-memory directory
├── ServiceResult.cs                — success/failure result returned by the facade
├── Subsystems.cs                   — the four services the facade coordinates
├── OffboardingFacade.cs            — FACADE (one operation over the subsystems)
├── Program.cs                      — composition root and demo scenarios
└── README.md
```

## How to Run

```bash
cd src/facade-design-pattern
dotnet run
```

## Output

```
=== Facade Pattern — Employee Offboarding ===

FACADE      IOffboardingFacade.CompleteExitAsync(id, subscriptionId)
SUBSYSTEMS  IPersonnelService       GetExitIntentAsync
            IExitRecordsService     GetExitDatesAsync
            IAccountService         CloseAccountAsync
            IReportingLineService   RemoveFromReportingLineAsync

--- Without the facade the caller has to know all of this ---
    intent = await personnelService.GetExitIntentAsync(id);
    dates  = await exitRecordsService.GetExitDatesAsync(id);
    // ...decide eligibility from the intent, the date and today...
    await accountService.CloseAccountAsync(id, subscriptionId);
    await reportingLineService.RemoveFromReportingLineAsync(id);

--- With the facade the caller makes one call ---
  Ram Bahadur (retirement effective 30 days ago):
    SUCCESS: Employee offboarded successfully.
    account active: False, in reporting line: False

  Sita Sharma (resignation takes effect in 15 days):
    FAILED: The employee is not eligible to exit yet.
    account active: True, in reporting line: True   // untouched

  Hari Prasad (no exit request):
    FAILED: Exit cannot be started at this time.
    account active: True, in reporting line: True   // untouched

=== Key Insight ===
The facade adds no new behaviour. It hides a sequence, an eligibility rule and the
choice of subsystem behind one operation the caller can name. The caller never learns
that the account, the exit record and the reporting line must change together.
It does not hide the subsystems: code that needs one can still take it directly.
```

## The Code

### Employee.cs

```csharp
namespace FacadeDesignPattern;

public class Employee
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public bool IsResigning { get; set; }
    public bool IsRetiring { get; set; }
    public DateOnly? ResignationDate { get; set; }
    public DateOnly? RetirementDate { get; set; }
    public bool IsAccountActive { get; set; } = true;
    public bool IsInReportingLine { get; set; } = true;
}

public class InMemoryEmployeeDirectory
{
    private readonly Dictionary<int, Employee> _employees = [];

    public void Seed(params Employee[] employees)
    {
        foreach (var employee in employees)
            _employees[employee.Id] = employee;
    }

    public Employee Get(int employeeId) =>
        _employees.TryGetValue(employeeId, out var employee)
            ? employee
            : throw new InvalidOperationException($"Employee {employeeId} does not exist.");
}
```

### ServiceResult.cs

```csharp
namespace FacadeDesignPattern;

public class ServiceResult
{
    private ServiceResult(bool status, string message)
    {
        Status = status;
        Message = message;
    }

    public bool Status { get; }
    public string Message { get; }

    public static ServiceResult Success(string message) => new(true, message);
    public static ServiceResult Fail(string message) => new(false, message);

    public override string ToString() => $"{(Status ? "SUCCESS" : "FAILED")}: {Message}";
}
```

### Subsystems.cs — the services behind the facade

```csharp
namespace FacadeDesignPattern;

public interface IPersonnelService
{
    Task<(bool IsResigning, bool IsRetiring)> GetExitIntentAsync(int employeeId);
}

public class PersonnelService(InMemoryEmployeeDirectory directory) : IPersonnelService
{
    public Task<(bool IsResigning, bool IsRetiring)> GetExitIntentAsync(int employeeId)
    {
        var employee = directory.Get(employeeId);
        return Task.FromResult((employee.IsResigning, employee.IsRetiring));
    }
}

public interface IExitRecordsService
{
    Task<(DateOnly? RetirementDate, DateOnly? ResignationDate)> GetExitDatesAsync(int employeeId);
}

public class ExitRecordsService(InMemoryEmployeeDirectory directory) : IExitRecordsService
{
    public Task<(DateOnly? RetirementDate, DateOnly? ResignationDate)> GetExitDatesAsync(int employeeId)
    {
        var employee = directory.Get(employeeId);
        return Task.FromResult((employee.RetirementDate, employee.ResignationDate));
    }
}

public interface IAccountService
{
    Task<ServiceResult> CloseAccountAsync(int employeeId, Guid subscriptionId);
}

public class AccountService(InMemoryEmployeeDirectory directory) : IAccountService
{
    public Task<ServiceResult> CloseAccountAsync(int employeeId, Guid subscriptionId)
    {
        var employee = directory.Get(employeeId);
        if (!employee.IsAccountActive)
            return Task.FromResult(ServiceResult.Fail("Account is already closed."));

        employee.IsAccountActive = false;
        return Task.FromResult(ServiceResult.Success("Account closed."));
    }
}

public interface IReportingLineService
{
    Task RemoveFromReportingLineAsync(int employeeId);
}

public class ReportingLineService(InMemoryEmployeeDirectory directory) : IReportingLineService
{
    public Task RemoveFromReportingLineAsync(int employeeId)
    {
        directory.Get(employeeId).IsInReportingLine = false;
        return Task.CompletedTask;
    }
}
```

### OffboardingFacade.cs — the facade

```csharp
namespace FacadeDesignPattern;

public interface IOffboardingFacade
{
    Task<ServiceResult> CompleteExitAsync(int employeeId, Guid subscriptionId);
}

public sealed class OffboardingFacade(
    IAccountService accountService,
    IExitRecordsService exitRecordsService,
    IPersonnelService personnelService,
    IReportingLineService reportingLineService,
    DateOnly today) : IOffboardingFacade
{
    public async Task<ServiceResult> CompleteExitAsync(int employeeId, Guid subscriptionId)
    {
        var (isResigning, isRetiring) = await personnelService.GetExitIntentAsync(employeeId);
        if (!isResigning && !isRetiring)
            return ServiceResult.Fail("Exit cannot be started at this time.");

        var (retirementDate, resignationDate) = await exitRecordsService.GetExitDatesAsync(employeeId);

        var shouldCloseAccount = false;
        if (isRetiring)
        {
            if (retirementDate is null || retirementDate <= today)
                shouldCloseAccount = true;
        }
        else if (isResigning)
        {
            if (resignationDate is null || resignationDate <= today)
                shouldCloseAccount = true;
        }

        if (!shouldCloseAccount)
            return ServiceResult.Fail("The employee is not eligible to exit yet.");

        var result = await accountService.CloseAccountAsync(employeeId, subscriptionId);
        await reportingLineService.RemoveFromReportingLineAsync(employeeId);

        return result.Status
            ? ServiceResult.Success("Employee offboarded successfully.")
            : ServiceResult.Fail(result.Message);
    }
}
```

### Program.cs

```csharp
using FacadeDesignPattern;

Console.WriteLine("=== Facade Pattern — Employee Offboarding ===\n");
Console.WriteLine("FACADE      IOffboardingFacade.CompleteExitAsync(id, subscriptionId)");
Console.WriteLine("SUBSYSTEMS  IPersonnelService       GetExitIntentAsync");
Console.WriteLine("            IExitRecordsService     GetExitDatesAsync");
Console.WriteLine("            IAccountService         CloseAccountAsync");
Console.WriteLine("            IReportingLineService   RemoveFromReportingLineAsync\n");

var today = new DateOnly(2026, 10, 19);
var subscriptionId = Guid.Parse("11111111-1111-1111-1111-111111111111");

var directory = new InMemoryEmployeeDirectory();
directory.Seed(
    new Employee
    {
        Id = 42,
        Name = "Ram Bahadur",
        IsRetiring = true,
        RetirementDate = today.AddDays(-30)
    },
    new Employee
    {
        Id = 43,
        Name = "Sita Sharma",
        IsResigning = true,
        ResignationDate = today.AddDays(15)
    },
    new Employee
    {
        Id = 44,
        Name = "Hari Prasad"
    });

IOffboardingFacade facade = new OffboardingFacade(
    new AccountService(directory),
    new ExitRecordsService(directory),
    new PersonnelService(directory),
    new ReportingLineService(directory),
    today);

Console.WriteLine("--- Without the facade the caller has to know all of this ---");
Console.WriteLine("    intent = await personnelService.GetExitIntentAsync(id);");
Console.WriteLine("    dates  = await exitRecordsService.GetExitDatesAsync(id);");
Console.WriteLine("    // ...decide eligibility from the intent, the date and today...");
Console.WriteLine("    await accountService.CloseAccountAsync(id, subscriptionId);");
Console.WriteLine("    await reportingLineService.RemoveFromReportingLineAsync(id);\n");

Console.WriteLine("--- With the facade the caller makes one call ---");

Console.WriteLine("  Ram Bahadur (retirement effective 30 days ago):");
var ram = await facade.CompleteExitAsync(42, subscriptionId);
Console.WriteLine($"    {ram}");
Console.WriteLine($"    account active: {directory.Get(42).IsAccountActive}, in reporting line: {directory.Get(42).IsInReportingLine}\n");

Console.WriteLine("  Sita Sharma (resignation takes effect in 15 days):");
var sita = await facade.CompleteExitAsync(43, subscriptionId);
Console.WriteLine($"    {sita}");
Console.WriteLine($"    account active: {directory.Get(43).IsAccountActive}, in reporting line: {directory.Get(43).IsInReportingLine}   // untouched\n");

Console.WriteLine("  Hari Prasad (no exit request):");
var hari = await facade.CompleteExitAsync(44, subscriptionId);
Console.WriteLine($"    {hari}");
Console.WriteLine($"    account active: {directory.Get(44).IsAccountActive}, in reporting line: {directory.Get(44).IsInReportingLine}   // untouched\n");

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("The facade adds no new behaviour. It hides a sequence, an eligibility rule and the");
Console.WriteLine("choice of subsystem behind one operation the caller can name. The caller never learns");
Console.WriteLine("that the account, the exit record and the reporting line must change together.");
Console.WriteLine("It does not hide the subsystems: code that needs one can still take it directly.");
```

## Real-World Analogy

Think of a hotel front desk. A guest asks for "a late checkout and a taxi at six". The desk calls housekeeping, the billing system and the taxi company in the right order and answers with a single confirmation. The guest never learns which department does what, or that housekeeping must approve the late checkout before billing can adjust the bill. The departments still exist and can be contacted directly when a guest has a request the desk does not cover.

## When to Use

- A use case needs several collaborators called in a specific order, and every caller would otherwise repeat that sequence
- You want to give a common operation one clear name instead of exposing four services to every controller or handler
- Eligibility, validation or translation rules belong together in one place rather than in each caller
- You are adding a simpler entry point over a subsystem that is already correct and should not be edited
- You do **not** want to hide the subsystems entirely: a caller with a different need can still use them directly
