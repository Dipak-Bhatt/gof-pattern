using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Colleagues;

/// <summary>
/// A request module that can load one of its requests and apply an approval decision.
/// Colleagues never call each other; they only answer the mediator.
/// </summary>
public interface IApprovalColleague
{
    ApprovalFlowType FlowType { get; }

    string ModuleName { get; }

    ApprovalRequest? Find(int requestId);

    ApprovalResult Approve(ApprovalRequest request, string note);

    ApprovalResult Reject(ApprovalRequest request, string note);
}
