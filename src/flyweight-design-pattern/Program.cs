using FlyweightDesignPattern;

Console.WriteLine("=== Flyweight Pattern — Shared RLS Prefix ===\n");
Console.WriteLine("INTRINSIC   RlsShape + CompiledPrefix: setting names and compiled SQL, one per shape");
Console.WriteLine("EXTRINSIC   RlsContext / RlsCommand: the tenant and user values, per request\n");

var tenantAndUser = RlsShape.Create("app.tenant_id", "app.user_id");
var tenantOnly = RlsShape.Create("app.tenant_id");

RlsContext TwoSettings(RlsShape shape, string tenant, string user) => new(shape, tenant, user);
RlsContext OneSetting(RlsShape shape, string tenant) => new(shape, tenant);

var factory = new RlsPrefix();

// 1,001 requests with the same shape: each allocates a context and a command, but all of them
// share one CompiledPrefix.
var first = factory.Resolve(tenantAndUser);
for (var i = 1; i <= 1000; i++)
{
    var context = TwoSettings(tenantAndUser, $"tenant-{i:D4}", $"user-{i:D4}");
    var command = factory.Compile(context);
    if (!ReferenceEquals(command.Prefix, first))
    {
        throw new InvalidOperationException("A new prefix object was allocated for the same shape.");
    }
}

// A different shape (a different ordered set of names) gets its own shared object.
var other = factory.Compile(OneSetting(tenantOnly, "tenant-0001")).Prefix;

Console.WriteLine("Requests: 1,001 two-setting contexts + 1 one-setting context");
Console.WriteLine($"shared CompiledPrefix objects : {factory.SharedObjectCount}");
Console.WriteLine($"same shape reused by reference : {ReferenceEquals(first, factory.Resolve(tenantAndUser))}");
Console.WriteLine($"different shape is distinct    : {!ReferenceEquals(first, other)}\n");

Console.WriteLine("Shared SQL (intrinsic):");
Console.WriteLine($"  {factory.SqlFor(tenantAndUser)}\n");

Console.WriteLine("Same shared prefix, different values each request (extrinsic):");
Console.WriteLine($"  {factory.Compile(TwoSettings(tenantAndUser, "tenant-0001", "user-0001")).Render()}");
Console.WriteLine($"  {factory.Compile(TwoSettings(tenantAndUser, "tenant-0002", "user-0002")).Render()}\n");

// Warm up, then measure the path that only resolves the shared prefix.
factory.Resolve(tenantAndUser);
var before = GC.GetAllocatedBytesForCurrentThread();
for (var i = 0; i < 10_000; i++)
{
    _ = factory.Resolve(tenantAndUser);
}

var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
Console.WriteLine($"Resolving the same shape 10,000 times: {allocated:N0} bytes allocated");
Console.WriteLine("(contexts and commands still allocate per request; the compiled prefix does not)\n");

Console.WriteLine("=== Key Insight ===");
Console.WriteLine("The SQL depends only on the shape (which setting names exist), so it is built once per");
Console.WriteLine("shape and shared. The values depend on the request, so they stay outside the shared");
Console.WriteLine("object and are bound on each call. Many contexts, two objects.");
