using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Colleagues;

public abstract class ApprovalColleagueBase : IApprovalColleague
{
    private readonly Dictionary<int, ApprovalRequest> _requests;

    protected ApprovalColleagueBase(params ApprovalRequest[] requests)
    {
        _requests = requests.ToDictionary(request => request.Id);
    }

    public abstract ApprovalFlowType FlowType { get; }

    public abstract string ModuleName { get; }

    public ApprovalRequest? Find(int requestId) =>
        _requests.GetValueOrDefault(requestId);

    public ApprovalResult Approve(ApprovalRequest request, string note)
    {
        request.Status = ApprovalStatus.Approved;

        return ApprovalResult.Ok(
            $"[{ModuleName}] {request.Employee}'s {request.Description} approved and applied. Note: \"{note}\"");
    }

    public ApprovalResult Reject(ApprovalRequest request, string note)
    {
        request.Status = ApprovalStatus.Rejected;

        return ApprovalResult.Ok(
            $"[{ModuleName}] {request.Employee}'s {request.Description} rejected; nothing applied. Note: \"{note}\"");
    }
}
