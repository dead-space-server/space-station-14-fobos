using Content.Shared.DeadSpace.Humanoid.Markings;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings
{
    [Prototype]
    public sealed partial class MarkingPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = "uwu";

        public string Name { get; private set; } = default!;

        [DataField("bodyPart", required: true)]
        public HumanoidVisualLayers BodyPart { get; private set; } = default!;

        [DataField("markingCategory", required: true)]
        public MarkingCategories MarkingCategory { get; private set; } = default!;

        [DataField("speciesRestriction")]
        public List<string>? SpeciesRestrictions { get; private set; }

        // DS14-sponsors-start
        [DataField("sponsorOnly")]
        public bool SponsorOnly = false;
        // DS14-sponsors-end

        [DataField("sexRestriction")]
        public Sex? SexRestriction { get; private set; }

        [DataField("followSkinColor")]
        public bool FollowSkinColor { get; private set; } = false;

        [DataField("forcedColoring")]
        public bool ForcedColoring { get; private set; } = false;

        [DataField("coloring")]
        public MarkingColors Coloring { get; private set; } = new();

        /// <summary>
        /// Do we need to apply any displacement maps to this marking? Set to false if your marking is incompatible
        /// with a standard human doll, and is used for some special races with unusual shapes
        /// </summary>
        [DataField]
        public bool CanBeDisplaced { get; private set; } = true;

        // DS14-start
        [DataField("sprites", required: true, customTypeSerializer: typeof(MarkingSpriteLayerListSerializer))]
        public List<MarkingSpriteLayer> SpriteLayers { get; private set; } = new();

        private IReadOnlyList<SpriteSpecifier>? _sprites;
        public IReadOnlyList<SpriteSpecifier> Sprites => _sprites ??= SpriteLayers.ConvertAll(layer => layer.Sprite);
        // DS14-end

        public Marking AsMarking()
        {
            return new Marking(ID, Sprites.Count);
        }
    }
}
