using Content.Server.Silicons.Laws;
using Content.Shared.DeadSpace.LawConfigurator.Systems;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;

namespace Content.Server.DeadSpace.LawConfigurator;

public sealed class LawConfiguratorServerSystem : EntitySystem
{
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<ConfigureLawsFromBoardEvent>(OnConfigureLawsFromBoard);
    }
    
    private void OnConfigureLawsFromBoard(ConfigureLawsFromBoardEvent args)
    {
        if (!TryComp<SiliconLawProviderComponent>(args.Board, out var boardLawProvider))
            return;
        
        var lawset = _siliconLaw.GetLawset(boardLawProvider.Laws);
        _siliconLaw.SetLaws(lawset.Laws, args.Target, boardLawProvider.LawUploadSound);
        
        // Флаг Subverted останется прежним
        // Если синтетик был подчинен, он останется помеченным
    }
}