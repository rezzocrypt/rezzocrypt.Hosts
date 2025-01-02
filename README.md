## `installation`

```shell
dotnet add package rezzocrypt.Hosts
```

## `usage`

**Editing the hosts file on Windows.**

```csharp
using rezzocrypt.Hosts;

// open the file
var file = Hosts.OpenFile(/* optional file path */);

```
