namespace AdapterDesignPattern;

/// <summary>
/// TARGET — what the client asks for. An id and nothing else: the tenant is already implied by the
/// connection, the way every service was written before its tables moved to PostgreSQL.
/// </summary>
public interface IEmployeeReader
{
    Task<Employee?> ReadAsync(int id);
}