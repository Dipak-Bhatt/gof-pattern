using Microsoft.Extensions.DependencyInjection;
using SingletonDesignPattern.DependencyInjection;
using SingletonDesignPattern.Domain;

Console.WriteLine("=== Singleton Pattern Demo ===\n");

// 1. Classic Singleton, explained simply.
//    Two different places ask for the logger. They get the SAME object.
Console.WriteLine("--- 1. Classic Singleton, explained simply ---");
Console.WriteLine("Two different places ask for the logger.\n");

var loggerA = SimpleLogger.Instance;
var loggerB = SimpleLogger.Instance;

loggerA.Log("App started");
loggerB.Log("User logged in");

Console.WriteLine();
Console.WriteLine($"Same object? {ReferenceEquals(loggerA, loggerB)}");
Console.WriteLine($"Messages stored in the one logger: {loggerA.Messages.Count}\n");

// 2. Thread-safe classic Singleton (Lazy<T>).
//    The connection pool is a real resource, so it is built once and safely.
Console.WriteLine("--- 2. Thread-safe classic Singleton (Lazy<T>) ---");
var first = TenantConnectionPool.Instance;
var second = TenantConnectionPool.Instance;
Console.WriteLine($"Same instance: {ReferenceEquals(first, second)}");
Console.WriteLine($"Connections opened through it: {first.Open()} then {second.Open()}\n");

// 3. The DI singleton — the form .NET apps actually use.
//    Register once; every scope gets the same object.
Console.WriteLine("--- 3. DI singleton (the form .NET apps use) ---");
var services = new ServiceCollection();
services.AddTenantConnectionPool();
var provider = services.BuildServiceProvider();

using (var scopeA = provider.CreateScope())
using (var scopeB = provider.CreateScope())
{
    var fromA = scopeA.ServiceProvider.GetRequiredService<TenantConnectionPool>();
    var fromB = scopeB.ServiceProvider.GetRequiredService<TenantConnectionPool>();
    Console.WriteLine($"Same across scopes: {ReferenceEquals(fromA, fromB)}");
    Console.WriteLine($"Connections opened through DI: {fromA.Open()} then {fromB.Open()}");
}
