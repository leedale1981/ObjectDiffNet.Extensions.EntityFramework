using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ObjectDiffNet.Common;

namespace ObjectDiffNet.Extensions.EntityFramework.Tests;

public class ObjectDiffTests : IAsyncLifetime
{
    private IEnumerable<Difference> _differences; 
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "$trongPassword";
    private const ushort MsSqlPort = 1433;
    private IContainer _container;
    private TestDbContext _context;
    
    private async Task CreateDatabaseWithRecord()
    {
        _container = new ContainerBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MsSqlPort, true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SQLCMDUSER", Username)
            .WithEnvironment("SQLCMDPASSWORD", Password)
            .WithEnvironment("MSSQL_SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();
        
        await _container.StartAsync();
        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(MsSqlPort);
        
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer($"Server={host},{port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True;")
            .Options;
        
        _context = new TestDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _context.TestEntities.Add(new TestClass()
        {
            StringProperty = "Test",
            IntProperty = 1,
            BoolProperty = true,
            DateProperty = new DateTime(2021, 1, 1),
            DecimalProperty = 1.1m
        });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public void Differences_Not_Empty()
    {
        Assert.NotEmpty(_differences);
    }
    
    [Fact]
    public void Differences_Count_Is_Correct()
    {
        Assert.Equal(4, _differences.Count());
    }
    
    [Fact]
    public void Differences_StringProperty_Has_Correct_Difference()
    {
        Assert.Contains(new("TestClass","StringProperty", "Test", "Test2", typeof(string)), 
            _differences);
    }
    
    [Fact]
    public void Differences_IntProperty_Has_Correct_Difference()
    {
        Assert.Contains(new("TestClass","IntProperty", "1", "2", typeof(int)), 
            _differences);
    }
    
    [Fact]
    public void Differences_BoolProperty_Has_Correct_Difference()
    {
        Assert.Contains(new("TestClass","BoolProperty", "True", "False", typeof(bool)), 
            _differences);
    }
    
    [Fact]
    public void Differences_DateProperty_Has_Correct_Difference()
    {
        Assert.Contains(new("TestClass","DateProperty", "01/01/2021 00:00:00", "01/02/2021 00:00:00", typeof(DateTime)), 
            _differences);
    }
    
    [Fact]
    public void Differences_DecimalProperty_Not_In_Differences()
    {
        Assert.DoesNotContain(new("TestClass","DecimalProperty", "1.1", "1.1", typeof(decimal)), 
            _differences);
    }

    public async Task InitializeAsync()
    {
        await CreateDatabaseWithRecord();

        TestClass? modifiedTestClass = await _context.TestEntities.FindAsync( (long)1);
        modifiedTestClass.StringProperty = "Test2";
        modifiedTestClass.BoolProperty = false;
        modifiedTestClass.DateProperty = new DateTime(2021, 2, 1);
        modifiedTestClass.IntProperty = 2;
        EntityEntry entityEntry = _context.ChangeTracker.Entries().FirstOrDefault();
        
        IDiffer diff = new Differ();
        _differences = diff.GetDifferences(entityEntry);
    }

    public async Task DisposeAsync()
    {
        _context.Dispose();
        await _container.StopAsync();
    }
}