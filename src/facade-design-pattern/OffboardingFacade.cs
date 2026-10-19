namespace FacadeDesignPattern;

public interface IOffboardingFacade
{
    Task<ServiceResult> CompleteExitAsync(int employeeId, Guid subscriptionId);
}

public sealed class OffboardingFacade(
    IAccountService accountService,
    IExitRecordsService exitRecordsService,
    IPersonnelService personnelService,
    IReportingLineService reportingLineService,
    DateOnly today) : IOffboardingFacade
{
    public async Task<ServiceResult> CompleteExitAsync(int employeeId, Guid subscriptionId)
    {
        var (isResigning, isRetiring) = await personnelService.GetExitIntentAsync(employeeId);
        if (!isResigning && !isRetiring)
            return ServiceResult.Fail("Exit cannot be started at this time.");

        var (retirementDate, resignationDate) = await exitRecordsService.GetExitDatesAsync(employeeId);

        var shouldCloseAccount = false;
        if (isRetiring)
        {
            if (retirementDate is null || retirementDate <= today)
                shouldCloseAccount = true;
        }
        else if (isResigning)
        {
            if (resignationDate is null || resignationDate <= today)
                shouldCloseAccount = true;
        }

        if (!shouldCloseAccount)
            return ServiceResult.Fail("The employee is not eligible to exit yet.");

        var result = await accountService.CloseAccountAsync(employeeId, subscriptionId);
        await reportingLineService.RemoveFromReportingLineAsync(employeeId);

        return result.Status
            ? ServiceResult.Success("Employee offboarded successfully.")
            : ServiceResult.Fail(result.Message);
    }
}
