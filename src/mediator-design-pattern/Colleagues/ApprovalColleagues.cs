using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Colleagues;

public sealed class LeaveApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.Leave;
    public override string ModuleName => "Leave";
}

public sealed class TimeApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.Time;
    public override string ModuleName => "Time";
}

public sealed class TravelApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.Travel;
    public override string ModuleName => "Travel";
}

public sealed class AdvanceApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.Advance;
    public override string ModuleName => "Advance";
}

public sealed class ExpenseClaimApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.ExpenseClaim;
    public override string ModuleName => "Expense Claim";
}

public sealed class OvertimeApprovalColleague(params ApprovalRequest[] requests)
    : ApprovalColleagueBase(requests)
{
    public override ApprovalFlowType FlowType => ApprovalFlowType.Overtime;
    public override string ModuleName => "Overtime";
}
