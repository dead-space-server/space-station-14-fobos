// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Content.Shared.DeadSpace.Virus.Components;
using Content.Server.DeadSpace.Virus.Symptoms;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Virus;

namespace Content.Server.DeadSpace.Virus.Systems;

public sealed partial class VirusSystem : SharedVirusSystem
{
    public void AsphyxiaInitialize()
    {
        SubscribeLocalEvent<VirusComponent, InhaleLocationEvent>(OnInhale);
    }

    private void OnInhale(Entity<VirusComponent> ent, ref InhaleLocationEvent args)
    {
        if (!HasSymptom<AsphyxiaSymptom>((ent.Owner, ent.Comp)))
            return;

        if (!CanManifestInHost((ent, ent.Comp)))
            return;

        if (TryComp<InternalsComponent>(ent, out var internalsComponent))
        {
            if (internalsComponent.BreathTools.Count <= 0)
                args.Gas = null; // воздуха нет
        }
    }
}