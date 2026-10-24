using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

/// <summary>
/// Records approval progress. The mediator calls it so no request module has to
/// know how levels or tracking details work.
/// </summary>
public interface IApprovalTracker
{
    ApprovalResult ApproveAllLevels(ApprovalRequest request);

    ApprovalResult RejectCurrentLevel(ApprovalRequest request, string note);
}
