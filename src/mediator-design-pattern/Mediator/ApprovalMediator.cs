using MediatorDesignPattern.Colleagues;
using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

public sealed class ApprovalMediator(
    IApprovalTracker tracker,
    IApproverNotifier notifier) : IApprovalMediator
{
    private readonly Dictionary<ApprovalFlowType, IApprovalColleague> _colleagues = [];

    public void Register(IApprovalColleague colleague) =>
        _colleagues[colleague.FlowType] = colleague;

    public ApprovalResult Approve(int requestId, ApprovalFlowType flowType, string note) =>
        Decide(requestId, flowType, note, isApproval: true);

    public ApprovalResult Reject(int requestId, ApprovalFlowType flowType, string note) =>
        Decide(requestId, flowType, note, isApproval: false);

    private ApprovalResult Decide(
        int requestId,
        ApprovalFlowType flowType,
        string note,
        bool isApproval)
    {
        if (!_colleagues.TryGetValue(flowType, out var colleague))
            return ApprovalResult.Fail($"No colleague is registered for {flowType}.");

        var request = colleague.Find(requestId);
        if (request is null)
            return ApprovalResult.Fail($"Request {requestId} was not found in {colleague.ModuleName}.");

        Console.WriteLine(
            $"  mediator: sending request {requestId} to the {colleague.ModuleName} colleague");

        var tracking = isApproval
            ? tracker.ApproveAllLevels(request)
            : tracker.RejectCurrentLevel(request, note);

        if (!tracking.Success)
            return tracking;

        var applied = isApproval
            ? colleague.Approve(request, note)
            : colleague.Reject(request, note);

        if (!applied.Success)
            return applied;

        notifier.Notify(request, request.Status);
        return applied;
    }
}
