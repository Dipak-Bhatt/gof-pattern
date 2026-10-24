using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

/// <summary>
/// Sends the decision back to the employee. The request modules do not know this
/// service exists; the mediator wires it into the flow.
/// </summary>
public interface IApproverNotifier
{
    void Notify(ApprovalRequest request, ApprovalStatus status);
}
