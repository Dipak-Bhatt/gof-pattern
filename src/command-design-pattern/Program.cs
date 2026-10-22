using CommandDesignPattern.Immediate;
using CommandDesignPattern.Models;
using CommandDesignPattern.Queued;
using CommandDesignPattern.Receivers;

var period = new PayrollPeriod(2026, 10);

Console.WriteLine("=== Command Pattern — Design B: immediate execution ===\n");
Console.WriteLine("CHAIN  Controller -> Command -> PayrollService");
Console.WriteLine("The controller creates each command and executes it immediately.\n");

var immediatePayroll = new PayrollService(employeeCount: 3);
var immediateController = new ImmediatePayrollController(immediatePayroll);

await immediateController.SubmitAsync(period);

Console.WriteLine(
    $"\nPayroll run complete: locked={immediatePayroll.IsLocked}, " +
    $"gross={immediatePayroll.GrossTotal:0}, net={immediatePayroll.NetTotal:0}, " +
    $"payslips={immediatePayroll.PayslipsGenerated}, emailed={immediatePayroll.PayslipsEmailed}.");

Console.WriteLine("\n=== Command Pattern — Design C: background queue ===\n");
Console.WriteLine("CHAIN  Controller -> Command -> Queue -> Worker -> PayrollService");
Console.WriteLine("The controller only enqueues; a worker executes the commands later.\n");

var queuedPayroll = new PayrollService(employeeCount: 3);
var queue = new InMemoryPayrollCommandQueue();
var queuedController = new QueuedPayrollController(queue);
var worker = new PayrollWorker(queue, queuedPayroll);

queuedController.Submit(period);
Console.WriteLine(
    $"Queue depth: {queue.Count}; no work has run yet. " +
    $"locked={queuedPayroll.IsLocked}, payslips={queuedPayroll.PayslipsGenerated}\n");

await worker.RunAsync();

Console.WriteLine(
    $"\nPayroll run complete: locked={queuedPayroll.IsLocked}, " +
    $"gross={queuedPayroll.GrossTotal:0}, net={queuedPayroll.NetTotal:0}, " +
    $"payslips={queuedPayroll.PayslipsGenerated}, emailed={queuedPayroll.PayslipsEmailed}.");

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Both designs use the same commands and the same receiver. Design B executes each");
Console.WriteLine("command inside the controller call. Design C puts the commands in a queue, so the");
Console.WriteLine("controller can return before any payroll work happens; the worker owns execution.");
