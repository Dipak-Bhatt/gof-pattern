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
