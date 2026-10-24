using MediatorDesignPattern.Models;

namespace MediatorDesignPattern.Mediator;

public sealed class ApprovalTracker : IApprovalTracker
{
    public ApprovalResult ApproveAllLevels(ApprovalRequest request)
    {
        while (request.NextApprovalLevel <= request.ApprovalLevels)
        {
            Console.WriteLine(
                $"    tracker: level {request.NextApprovalLevel}/{request.ApprovalLevels} approved");

            request.ApprovedLevels = request.NextApprovalLevel;
            request.NextApprovalLevel++;
        }

        return ApprovalResult.Ok("All approval levels recorded.");
    }

    public ApprovalResult RejectCurrentLevel(ApprovalRequest request, string note)
    {
        Console.WriteLine(
            $"    tracker: level {request.NextApprovalLevel}/{request.ApprovalLevels} rejected (note: \"{note}\")");

        return ApprovalResult.Ok("Rejection recorded.");
    }
}
