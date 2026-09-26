# Pronium Plugin Development Guide

## Project Structure
- Main plugin code: `Jellyfin.Plugin.Pronium/` (C#/.NET 8)
- Tests: `Jellyfin.Plugin.Pronium.Tests/` (NUnit tests)
- The plugin targets both Jellyfin and Emby with different configurations

## Key Commands
- Build: `dotnet build`
- Test specific test class: `dotnet test --filter "ClassName"`
- Test all: `dotnet test`
- Clean: `dotnet clean`

## Important Notes
- Plugin uses .NET 8 framework with conditional compilation for Jellyfin vs Emby targets
- Uses MSBuild project files with different configurations (Debug, Release, Debug.Emby, Release.Emby)
- Build artifacts are placed in `bin/` and `obj/` directories
- Test projects reference the main plugin project
- CI runs tests on Ubuntu with .NET 8

## Plugin Architecture
- Main entry point: `Plugin.cs`
- Configuration: `Configuration/PluginConfiguration.cs`
- Sites implemented in `Sites/` directory
- Scheduled tasks in `ScheduledTasks/`
- Extensions in `Extensions/`
- Helpers in `Helpers/`