using ChainOfResponsibilityDesignPattern;
using ChainOfResponsibilityDesignPattern.Handlers;

// The chain is assembled once, in the order the checks must run.
// Each handler decides whether to answer or to pass the request to the next one.
var pipeline = new RequestPipeline(
    new TenantContextHandler(),
    new SessionValidityHandler(),
    new SessionTagHandler(),
    new EndpointHandler());

Console.WriteLine("=== Chain of Responsibility Pattern — request pipeline ===\n");

await RunAsync(
    "1) Healthy request",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = true,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-a:ram"
    });

await RunAsync(
    "2) No tenant — an API call",
    pipeline,
    new RequestContext
    {
        SessionIsValid = true
    });

await RunAsync(
    "3) No tenant — a browser navigation",
    pipeline,
    new RequestContext
    {
        SessionIsValid = true,
        PrefersHtml = true
    });

await RunAsync(
    "4) Expired session",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = false,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-a:ram"
    });

await RunAsync(
    "5) Session replaced in another tab",
    pipeline,
    new RequestContext
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        SessionIsValid = true,
        SessionTagSent = "tenant-a:ram",
        SessionTagCurrent = "tenant-b:sita"
    });

static async Task RunAsync(string title, RequestPipeline pipeline, RequestContext context)
{
    await pipeline.HandleAsync(context);

    Console.WriteLine(title);
    Console.WriteLine($"   {context.Response.StatusCode}: {context.Response.Body}");
    Console.WriteLine();
}
