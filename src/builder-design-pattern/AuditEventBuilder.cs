using System.Text.Json;

namespace BuilderDesignPattern;

/// <summary>
/// Concrete Builder. Owns the partially assembled product and validates it in <see cref="Build"/>.
/// </summary>
public sealed class AuditEventBuilder
{
    private readonly IAuditLogger _logger;
    private AuditCategory? _category;
    private LogAction? _action;
    private AuditTarget _target = new();
    private string? _message;
    private object? _metadata;
    private string? _beforeState;
    private string? _afterState;
    private Guid? _tenantId;
    private int? _employeeId;

    public AuditEventBuilder(IAuditLogger logger, LogAction? action = null)
    {
        _logger = logger;
        _action = action;
    }

    public AuditEventBuilder SetCategory(AuditCategory category)
    {
        _category = category;
        return this;
    }

    public AuditEventBuilder SetAction(LogAction action)
    {
        _action = action;
        return this;
    }

    public AuditEventBuilder SetTarget(object? id, string? name = null)
    {
        _target = new AuditTarget(id, name);
        return this;
    }

    public AuditEventBuilder SetMessage(string? message)
    {
        _message = message;
        return this;
    }

    public AuditEventBuilder SetMetadata(object? metadata)
    {
        _metadata = metadata;
        return this;
    }

    public AuditEventBuilder SetBefore(object? state)
    {
        _beforeState = Serialize(state);
        return this;
    }

    public AuditEventBuilder SetAfter(object? state)
    {
        _afterState = Serialize(state);
        return this;
    }

    public AuditEventBuilder SetTenant(Guid? tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public AuditEventBuilder SetEmployee(int? employeeId)
    {
        _employeeId = employeeId;
        return this;
    }

    /// <summary>
    /// Validates the required parts and returns the immutable product. A half-built event
    /// never reaches the receiver.
    /// </summary>
    public AuditEvent Build()
    {
        if (_category is null)
            throw new InvalidOperationException("An audit category must be set before building.");

        if (_action is null)
            throw new InvalidOperationException("An audit action must be set before building.");

        var target = _target;
        if (target.Name is null && _action is LogAction.Added or LogAction.Downloaded or LogAction.Uploaded)
            target = target with { Name = _category.Value.ToString() };

        var before = _beforeState;
        var after = _afterState;
        if (_action == LogAction.Updated && (before is null || after is null || before == after))
            (before, after) = (null, null);

        return new AuditEvent
        {
            Category = _category.Value,
            Action = _action.Value,
            Target = target,
            Message = _message,
            Metadata = _metadata,
            BeforeState = before,
            AfterState = after,
            TenantId = _tenantId,
            EmployeeId = _employeeId
        };
    }

    public void Publish() => _logger.Write(Build());

    private static string? Serialize(object? state) => state is null ? null : JsonSerializer.Serialize(state);
}
