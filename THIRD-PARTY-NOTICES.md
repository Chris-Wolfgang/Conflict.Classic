# Third-Party Notices

Conflict.Classic is licensed under the MIT License (see [LICENSE](LICENSE)). It ships the
third-party components listed below, each under its own license. Only components that end up in
shipped output are listed. Build-time-only packages (analyzers, source generators such as
PolySharp, test frameworks, and `Microsoft.AspNetCore.Components.WebAssembly.DevServer`) are not
redistributed and are not listed.

Keep this file in step with the `PackageReference` items in `src/`. When a shipped dependency is
added, removed or bumped, update its row.

## Wolfgang.Conflict.Classic.Engine

The `net10.0` build has no third-party runtime dependencies. The `netstandard2.0` build references
these packages, plus their transitive dependencies:

| Package | Version | License | Copyright | Source |
|---|---|---|---|---|
| System.Memory | 4.6.3 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/maintenance-packages |
| Microsoft.Bcl.HashCode | 6.0.0 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/maintenance-packages |
| System.Text.Json | 8.0.5 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/runtime |
| Microsoft.Bcl.AsyncInterfaces (transitive) | 8.0.0 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/runtime |
| System.Text.Encodings.Web (transitive) | 8.0.0 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/runtime |
| System.Buffers (transitive) | 4.6.1 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/maintenance-packages |
| System.Numerics.Vectors (transitive) | 4.6.1 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/maintenance-packages |
| System.Runtime.CompilerServices.Unsafe (transitive) | 6.1.2 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/maintenance-packages |
| System.Threading.Tasks.Extensions (transitive) | 4.5.4 | MIT | .NET Foundation and Contributors | https://github.com/dotnet/corefx |

## Wolfgang.Conflict.Classic.UI.Blazor

A Blazor WebAssembly app. It sends the compiled assemblies below, together with the .NET
WebAssembly runtime (`Microsoft.NET.Sdk.WebAssembly.Pack`, MIT), to the browser. All are MIT,
copyright .NET Foundation and Contributors, from https://github.com/dotnet/aspnetcore or
https://github.com/dotnet/runtime.

| Package | Version | License |
|---|---|---|
| Microsoft.AspNetCore.Components.WebAssembly | 10.0.0 | MIT |
| Microsoft.AspNetCore.Components (transitive) | 10.0.0 | MIT |
| Microsoft.AspNetCore.Components.Forms (transitive) | 10.0.0 | MIT |
| Microsoft.AspNetCore.Components.Web (transitive) | 10.0.0 | MIT |
| Microsoft.AspNetCore.Authorization (transitive) | 10.0.0 | MIT |
| Microsoft.AspNetCore.Metadata (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.Configuration, .Abstractions, .Binder, .FileExtensions, .Json (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.DependencyInjection, .Abstractions (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.Diagnostics, .Abstractions (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.FileProviders.Abstractions, .Physical; Microsoft.Extensions.FileSystemGlobbing (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.Logging, .Abstractions (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.Options, .ConfigurationExtensions; Microsoft.Extensions.Primitives (transitive) | 10.0.0 | MIT |
| Microsoft.Extensions.Validation (transitive) | 10.0.0 | MIT |
| Microsoft.JSInterop, Microsoft.JSInterop.WebAssembly (transitive) | 10.0.0 | MIT |

## MIT License (applies to every component above)

```
The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```
