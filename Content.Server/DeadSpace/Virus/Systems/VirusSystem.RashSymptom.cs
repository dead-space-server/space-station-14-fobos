// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Server.DeadSpace.Virus.Symptoms;
using Robust.Shared.Physics.Events;
using Content.Shared.DeadSpace.Virus;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : SharedVirusSystem
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

        if (!CanManifestInHost((ent, ent.Comp)))
            return;

        ProbInfect((ent.Owner, ent.Comp), args.OtherEntity);
    }
}