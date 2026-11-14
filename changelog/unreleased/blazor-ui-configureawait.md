type: internal

The Blazor UI's two awaits outside components (`Program.cs` startup and `ConflictService.SaveLogAsync`) use `ConfigureAwait(false)`, so the project builds with CA2007 as an error.
