# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Space Station 14 (Fobos fork) - a C# remake of SS13 running on the Robust Toolbox engine. This is a Russian community fork of the main Space Wizards repository.

## Build and Run Commands

```bash
# Initial setup (run once after cloning)
python RUN_THIS.py

# Build
dotnet build --configuration DebugOpt

# Run server and client
./runclient.bat   # or runclient.sh on Linux
./runserver.bat   # or runserver.sh on Linux

# Run tests
dotnet test Content.Tests/Content.Tests.csproj -- NUnit.ConsoleOut=0
dotnet test Content.IntegrationTests/Content.IntegrationTests.csproj

# Run single test
dotnet test Content.Tests/Content.Tests.csproj --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Database migrations
./Content.Server.Database/add-migration.sh <MigrationName>
```

## Architecture

### Project Structure

- **Content.Shared** - Code running on both server and client (components, shared systems, prototypes)
- **Content.Server** - Server-only logic and systems
- **Content.Client** - Client-only logic, UI, rendering
- **RobustToolbox** - Game engine (git submodule)
- **Resources/Prototypes** - YAML entity and data definitions

### Entity Component System (ECS)

**Components** are data containers:
```csharp
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ExampleComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public int Value = 0;
}
```

Key attributes:
- `[RegisterComponent]` - Required for all components
- `[NetworkedComponent]` - Syncs to clients
- `[AutoGenerateComponentState]` - Generates serialization code
- `[DataField]` - Field loads from YAML prototypes
- `[AutoNetworkedField]` - Individual field syncs

**Systems** process components:
```csharp
public sealed class ExampleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExampleComponent, MapInitEvent>(OnInit);
    }
}
```

Pattern: Create shared system in Content.Shared, extend in Content.Server/Client if needed.

### Prototypes (YAML)

Entity definitions in `Resources/Prototypes/Entities/`:
```yaml
- type: entity
  id: ExampleEntity
  parent: BaseItem
  components:
  - type: Example
    value: 10
```

Data prototypes for configuration:
```yaml
- type: examplePrototype
  id: ExampleId
  name: example-name
```

Prototype C# definition:
```csharp
[Prototype]
public sealed partial class ExamplePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;
}
```

### Dependency Injection

Services register in IoC files:
- `Content.Shared/IoC/SharedContentIoC.cs`
- `Content.Server/IoC/ServerContentIoC.cs`

Usage: `[Dependency] private readonly IService _service = default!;`

### Network Events

```csharp
// Define event
[ByRefEvent]
public record struct ExampleEvent(EntityUid Entity);

// Raise event
RaiseLocalEvent(uid, ref exampleEvent);
```

## Code Conventions

- File-scoped namespaces: `namespace Content.Shared.Example;`
- Sealed partial classes for components
- Use `Dirty(uid, component)` to mark networked components as changed
- Access control: `[Access(typeof(ExampleSystem))]`
- Type-safe prototype references: `ProtoId<ExamplePrototype>`

## Testing

- Unit tests inherit from `ContentUnitTest`
- Integration tests use full game systems
- Test framework: NUnit
