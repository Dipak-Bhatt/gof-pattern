namespace MediatorDesignPattern.Models;

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public sealed class ApprovalRequest
{
    public required int Id { get; init; }
    public required ApprovalFlowType FlowType { get; init; }
    public required string Employee { get; init; }
    public required string Description { get; init; }
    public int ApprovalLevels { get; init; } = 3;

    public int NextApprovalLevel { get; internal set; } = 1;
    public int ApprovedLevels { get; internal set; }
    public ApprovalStatus Status { get; internal set; } = ApprovalStatus.Pending;
}
