# ObjectDiffNet
Provides extensions for the ObjectDiffNet library to make it easier to use with Microsoft Entity Framework.
## Usage
```csharp
using ObjectDiffNet;

IDiffer diff = new Differ();
EntityEntry entry = dbContext.ChangeTracker.Entries().FirstOrDefault();
IEnumerable<Difference> _differences = diff.GetDifferences(entry);

Assert.Contains(new("StringProperty", "Test", "Test2", typeof(string)), _differences);