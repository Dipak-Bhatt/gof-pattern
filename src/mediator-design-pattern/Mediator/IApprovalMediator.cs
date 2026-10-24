using MediatorDesignPattern.Colleagues;
using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

/// <summary>
/// The single entry point every caller uses. The caller supplies a request id and its
/// flow type; the mediator finds the right colleague and owns the shared steps.
/// </summary>
public interface IApprovalMediator
{
    void Register(IApprovalColleague colleague);

    ApprovalResult Approve(int requestId, ApprovalFlowType flowType, string note);

    ApprovalResult Reject(int requestId, ApprovalFlowType flowType, string note);
}
