# Virus Symptoms - Developer Guide

## Adding New Symptoms (After Refactor)

After the virus symptom refactor, adding new symptoms to downstream forks is much easier and doesn't require modifying core code.

### Step 1: Create Your Symptom Class

Create a new C# file in your fork (e.g., `MyCustomSymptom.cs`):

```csharp
using Content.Shared.DeadSpace.Virus.Symptoms;
using Content.Shared.DeadSpace.Virus.Components;
using Content.Shared.DeadSpace.TimeWindow;
using Content.Shared.DeadSpace.Virus.Prototypes;
using Robust.Shared.Prototypes;

namespace MyFork.Virus.Symptoms;

public sealed class MyCustomSymptom : VirusSymptomBase
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    
    // Reference your prototype ID
    protected override ProtoId<VirusSymptomPrototype> PrototypeId => "MyCustomSymptom";

    public MyCustomSymptom(TimedWindow effectTimedWindow) : base(effectTimedWindow)
    { }

    public override void OnAdded(EntityUid host, VirusComponent virus)
    {
        base.OnAdded(host, virus);
        // Add your custom logic here (e.g., add status effects)
    }

    public override void OnRemoved(EntityUid host, VirusComponent virus)
    {
        base.OnRemoved(host, virus);
        // Remove effects here
    }

    public override void DoEffect(EntityUid host, VirusComponent virus)
    {
        // This is called periodically - add your symptom's effect here
    }

    public override IVirusSymptom Clone()
    {
        return new MyCustomSymptom(EffectTimedWindow.Clone());
    }
}
```

### Step 2: Create Your Prototype

Create a YAML prototype (e.g., in `my-symptoms.yml`):

```yaml
- type: virusSymptom
  id: MyCustomSymptom
  name: My Custom Symptom Name
  description: What this symptom does
  symptomTypeClass: MyFork.Virus.Symptoms.MyCustomSymptom  # Fully qualified class name
  danger: Medium  # Low, Medium, High, or Cataclysm
  price: 1000  # Mutation cost
  addInfectivity: 0.05  # Optional: increase virus infectivity
  minInterval: 20  # Optional: minimum seconds between effects
  maxInterval: 60  # Optional: maximum seconds between effects
```

### Step 3: That's It!

No need to:
- ❌ Edit the `VirusSymptom` enum
- ❌ Modify the `CreateSymptomInstance()` switch statement
- ❌ Change any core files

The system will automatically:
- ✅ Load your symptom class via reflection
- ✅ Create instances when needed
- ✅ Identify symptoms by prototype ID

## Important Notes

1. **Fully Qualified Class Name**: The `symptomTypeClass` must include the complete namespace path to your class.

2. **Constructor**: Your symptom class MUST have a constructor that accepts `TimedWindow`.

3. **TypeId**: The `TypeId` property is automatically set to your prototype ID by the base class.

4. **Clone Method**: Always implement `Clone()` to create a new instance with a cloned time window.

## Examples

See existing symptoms in `Content.Server/DeadSpace/Virus/Symptoms/` for reference:
- `CoughSymptom.cs` - Simple status effect application
- `BlindableSymptom.cs` - Modifies existing components
- `LowComplexityChangeSymptom.cs` - Modifies virus data

## Migration from Old System

If you have symptoms using the old enum-based system, they will continue to work during the transition period. The prototype can specify both `symptomType` (old enum) and `symptomTypeClass` (new reflection). The system will prefer `symptomTypeClass` if provided.
