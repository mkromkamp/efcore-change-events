namespace EntityFrameworkCore.ChangeEvents.Tests;

public partial class ChangeEventInterceptorTests
{
    private readonly ChangeEventInterceptor _interceptor;
    private readonly TestContext _context;

    public ChangeEventInterceptorTests()
    {
        _interceptor = new ChangeEventInterceptor(new ChangeEventOptions());
        _context = new TestContext(new());
        _context.Database.EnsureCreated();
    }
}