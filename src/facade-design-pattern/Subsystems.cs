namespace FacadeDesignPattern;

public interface IPersonnelService
{
    Task<(bool IsResigning, bool IsRetiring)> GetExitIntentAsync(int employeeId);
}

public class PersonnelService(InMemoryEmployeeDirectory directory) : IPersonnelService
{
    public Task<(bool IsResigning, bool IsRetiring)> GetExitIntentAsync(int employeeId)
    {
        var employee = directory.Get(employeeId);
        return Task.FromResult((employee.IsResigning, employee.IsRetiring));
    }
}

public interface IExitRecordsService
{
    Task<(DateOnly? RetirementDate, DateOnly? ResignationDate)> GetExitDatesAsync(int employeeId);
}

public class ExitRecordsService(InMemoryEmployeeDirectory directory) : IExitRecordsService
{
    public Task<(DateOnly? RetirementDate, DateOnly? ResignationDate)> GetExitDatesAsync(int employeeId)
    {
        var employee = directory.Get(employeeId);
        return Task.FromResult((employee.RetirementDate, employee.ResignationDate));
    }
}

public interface IAccountService
{
    Task<ServiceResult> CloseAccountAsync(int employeeId, Guid subscriptionId);
}

public class AccountService(InMemoryEmployeeDirectory directory) : IAccountService
{
    public Task<ServiceResult> CloseAccountAsync(int employeeId, Guid subscriptionId)
    {
        var employee = directory.Get(employeeId);
        if (!employee.IsAccountActive)
            return Task.FromResult(ServiceResult.Fail("Account is already closed."));

        employee.IsAccountActive = false;
        return Task.FromResult(ServiceResult.Success("Account closed."));
    }
}

public interface IReportingLineService
{
    Task RemoveFromReportingLineAsync(int employeeId);
}

public class ReportingLineService(InMemoryEmployeeDirectory directory) : IReportingLineService
{
    public Task RemoveFromReportingLineAsync(int employeeId)
    {
        directory.Get(employeeId).IsInReportingLine = false;
        return Task.CompletedTask;
    }
}
