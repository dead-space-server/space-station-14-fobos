using System.Linq;
using Content.Shared.DeadSpace.UniformAccessories;
using Content.Shared.DeadSpace.UniformAccessories.Components;
using Content.Shared.Examine;
using Robust.Shared.Containers;

namespace Content.Server.DeadSpace.UniformAccessories;

public sealed class UniformAccessorySystem : SharedUniformAccessorySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UniformAccessoryHolderComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<UniformAccessoryHolderComponent, ExaminedEvent>(OnExamineAccessories);
    }

    private void OnTerminating(EntityUid holder,
        UniformAccessoryHolderComponent holderComp,
        ref EntityTerminatingEvent args)
    {
        var container = holderComp.AccessoryContainer;
        if (container == null || container.ContainedEntities.Count == 0)
            return;

        var transform = Transform(holder);
        var coordinates = transform.Coordinates;
        var accessories = container.ContainedEntities.ToArray();

        foreach (var accessory in accessories)
        {
            if (_container.Remove(accessory, container, reparent: false))
            {
                Transform(accessory).Coordinates = coordinates;
            }
        }
    }

    private void OnExamineAccessories(EntityUid holder, UniformAccessoryHolderComponent holderComp, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        var container = holderComp.AccessoryContainer;
        if (container == null || container.ContainedEntities.Count == 0)
            return;

        var accessories = new List<string>();
        foreach (var accessory in container.ContainedEntities)
        {
            if (!TryComp(accessory, out MetaDataComponent? metaData))
                continue;

            var colorHex = "#FFFF55";
            if (TryComp<UniformAccessoryComponent>(accessory, out var acc) && acc.Color != null)
                colorHex = acc.Color.Value.ToHex();

            accessories.Add($"[color={colorHex}]{metaData.EntityName}[/color]");
        }

        if (accessories.Count == 0)
            return;

        var accessoriesList = string.Join(", ", accessories);
        args.PushMarkup($"На этом предмете закреплено: {accessoriesList}.");
    }
}
