using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

public sealed class ApproverNotifier : IApproverNotifier
{
    public void Notify(ApprovalRequest request, ApprovalStatus status) =>
        Console.WriteLine(
            $"    notify: {request.Employee} — {request.FlowType} request {request.Id} is {status}");
}
