using Content.Shared.DeadSpace.Abilities;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Utility;

namespace Content.Client.DeadSpace.Abilities.Systems;

public sealed class RollingStoneVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly Dictionary<EntityUid, (string Path, string State)> _oldSprites = new();

    public override void Initialize()
    {
        base.Initialize();
        // Срабатывает ПОСЛЕ получения всех networked-полей с сервера
        SubscribeLocalEvent<ActiveRollingStoneComponent, AfterAutoHandleStateEvent>(OnStateReceived);
        SubscribeLocalEvent<ActiveRollingStoneComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStateReceived(Entity<ActiveRollingStoneComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (_oldSprites.ContainsKey(ent.Owner))
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (string.IsNullOrEmpty(ent.Comp.SpritePath) || string.IsNullOrEmpty(ent.Comp.SpriteState))
            return;

        // Сохраняем оригинал
        var layer = sprite[0];
        var oldPath = layer.Rsi?.Path.ToString() ?? string.Empty;
        var oldState = layer.RsiState.Name ?? string.Empty;
        _oldSprites[ent.Owner] = (oldPath, oldState);

        // Меняем RSI и стейт АТОМАРНО — передаём StateId вторым аргументом
        _sprite.LayerSetRsi((ent.Owner, sprite), 0, new ResPath(ent.Comp.SpritePath), new RSI.StateId(ent.Comp.SpriteState));
    }

    private void OnShutdown(Entity<ActiveRollingStoneComponent> ent, ref ComponentShutdown args)
    {
        if (!_oldSprites.TryGetValue(ent.Owner, out var old))
            return;

        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        // Восстанавливаем RSI и стейт АТОМАРНО
        if (!string.IsNullOrEmpty(old.Path) && !string.IsNullOrEmpty(old.State))
            _sprite.LayerSetRsi((ent.Owner, sprite), 0, new ResPath(old.Path), new RSI.StateId(old.State));

        _oldSprites.Remove(ent.Owner);
    }
}