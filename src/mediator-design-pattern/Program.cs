using MediatorDesignPattern.Colleagues;
using MediatorDesignPattern.Mediator;
using MediatorDesignPattern.Models;

Console.WriteLine("=== Mediator Pattern — Approval Workflow ===\n");
Console.WriteLine("MEDIATOR    ApprovalMediator");
Console.WriteLine("COLLEAGUES  Leave, Time, Travel, Advance, Expense Claim, Overtime");
Console.WriteLine("SHARED      record every approval level, apply the decision, notify the employee\n");

var leaveRequest = new ApprovalRequest
{
    Id = 101,
    FlowType = ApprovalFlowType.Leave,
    Employee = "Ram Bahadur",
    Description = "3 days of casual leave"
};

var timeRequest = new ApprovalRequest
{
    Id = 102,
    FlowType = ApprovalFlowType.Time,
    Employee = "Sita Sharma",
    Description = "late check-in correction"
};

var travelRequest = new ApprovalRequest
{
    Id = 201,
    FlowType = ApprovalFlowType.Travel,
    Employee = "Hari Prasad",
    Description = "field visit to Pokhara"
};

var advanceRequest = new ApprovalRequest
{
    Id = 202,
    FlowType = ApprovalFlowType.Advance,
    Employee = "Gita K.C.",
    Description = "NPR 20,000 salary advance"
};

var expenseRequest = new ApprovalRequest
{
    Id = 301,
    FlowType = ApprovalFlowType.ExpenseClaim,
    Employee = "Bikash Rai",
    Description = "NPR 4,500 client lunch"
};

IApprovalMediator mediator = new ApprovalMediator(
    new ApprovalTracker(),
    new ApproverNotifier());

mediator.Register(new LeaveApprovalColleague(leaveRequest));
mediator.Register(new TimeApprovalColleague(timeRequest));
mediator.Register(new TravelApprovalColleague(travelRequest));
mediator.Register(new AdvanceApprovalColleague(advanceRequest));
mediator.Register(new ExpenseClaimApprovalColleague(expenseRequest));

Console.WriteLine("--- Without the mediator, the caller owns the whole workflow ---");
Console.WriteLine("  switch (flowType)");
Console.WriteLine("  {");
Console.WriteLine("      case Leave:         await leaveService.ApproveAsync(...); break;");
Console.WriteLine("      case Time:          await timeService.ApproveAsync(...); break;");
Console.WriteLine("      case Travel:        await travelService.ApproveAsync(...); break;");
Console.WriteLine("      // ...one branch per module, each with its own load/approve/reject API");
Console.WriteLine("  }");
Console.WriteLine("  // and every branch still records levels and notifies the employee.\n");

Console.WriteLine("--- With the mediator, the caller makes one call ---");

ShowResult(
    "1. Approve a leave request",
    () => mediator.Approve(101, ApprovalFlowType.Leave, "approved by HR"));

ShowResult(
    "2. Reject an expense claim",
    () => mediator.Reject(301, ApprovalFlowType.ExpenseClaim, "receipt is missing"));

ShowResult(
    "3. Approve a travel request",
    () => mediator.Approve(201, ApprovalFlowType.Travel, "budget is available"));

ShowResult(
    "4. A flow type with no registered colleague",
    () => mediator.Approve(999, ApprovalFlowType.Overtime, "not handled yet"));

Console.WriteLine("\n--- 5. Add a new colleague at runtime ---");

var overtimeRequest = new ApprovalRequest
{
    Id = 401,
    FlowType = ApprovalFlowType.Overtime,
    Employee = "Anita Gurung",
    Description = "6 hours of overtime in October"
};

mediator.Register(new OvertimeApprovalColleague(overtimeRequest));

ShowResult(
    "   Approve the overtime request",
    () => mediator.Approve(401, ApprovalFlowType.Overtime, "pre-approved by the manager"));

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("The request modules never reference each other, the tracker or the notifier.");
Console.WriteLine("The caller never references a request module. Both sides talk only to the");
Console.WriteLine("mediator, which owns the order: find the colleague, record the levels, apply");
Console.WriteLine("the decision, then notify. Adding a request type is one colleague plus one");
Console.WriteLine("registration; no existing colleague and no caller has to change.");

static void ShowResult(string title, Func<ApprovalResult> action)
{
    Console.WriteLine($"\n--- {title} ---");
    var result = action();
    Console.WriteLine($"  result: {(result.Success ? "OK" : "FAILED")} — {result.Message}");
}
