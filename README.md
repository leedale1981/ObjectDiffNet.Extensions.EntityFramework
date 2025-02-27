# ObjectDiffNet
Simple library for generating differences between objects and their properties in .NET

## Usage
```csharp
using ObjectDiffNet;

IDiffer diff = new Differ();
IEnumerable<Difference> _differences = diff.GetDifferences(_object1, _object2);

Assert.Contains(new("StringProperty", "Test", "Test2", typeof(string)), _differences);