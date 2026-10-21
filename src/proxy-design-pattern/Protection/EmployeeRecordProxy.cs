namespace ProxyDesignPattern.Protection;

/// <summary>
/// PROTECTION PROXY — the GoF Proxy pattern used to control access. It implements the same
/// interface as the real record, forwards the open call, and checks the caller's role before
/// forwarding the protected ones. A caller without the right role is refused, and the real
/// subject is never reached.
/// </summary>
public sealed class EmployeeRecordProxy(
    IEmployeeRecord inner,
    Guid callerId,
    Role role) : IEmployeeRecord
{
    private readonly IEmployeeRecord _inner = inner;
    private readonly Guid _callerId = callerId;
    private readonly Role _role = role;

    /// <summary>Who may read or change salary. Everyone else is refused.</summary>
    private bool CanSeeSalary => _role is Role.Manager or Role.Hr;

    public string GetName() => _inner.GetName();

    public decimal GetSalary()
    {
        EnsureAllowed();
        return _inner.GetSalary();
    }

    public void SetSalary(decimal value)
    {
        EnsureAllowed();
        _inner.SetSalary(value);
    }

    private void EnsureAllowed()
    {
        if (CanSeeSalary)
            return;

        throw new UnauthorizedAccessException(
            $"Caller {_callerId} with role {_role} may not see or change salary.");
    }
}
