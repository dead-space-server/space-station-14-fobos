
using Content.Shared.DeadSpace.Virus.Components;
using Content.Server.DeadSpace.Virus.Symptoms;
using Robust.Shared.Physics.Events;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : EntitySystem
{
    public void RashInitialize()
    {
        SubscribeLocalEvent<VirusComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<VirusComponent> ent, ref StartCollideEvent args)
    {
        if (!HasSymptom<RashSymptom>((ent.Owner, ent.Comp)))
        {
            _sawmill.Debug($"[{ent.Owner}] не имеет симптома (RashSymptom)");
            return;
        }

        ProbInfect((ent.Owner, ent.Comp), args.OtherEntity);
    }
}