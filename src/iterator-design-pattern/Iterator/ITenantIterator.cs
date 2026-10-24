using IteratorDesignPattern.Models;

namespace IteratorDesignPattern.Iterator;

/// <summary>The Iterator specialised for a tenant run, so the aggregate's signature reads clearly.</summary>
public interface ITenantIterator : IIterator<Tenant>
{
}
