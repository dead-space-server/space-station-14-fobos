// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Numerics;
using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Utility;

namespace Content.Shared.DeadSpace.Humanoid.Markings;

public readonly record struct MarkingSpriteLayer(SpriteSpecifier Sprite, Vector2 Scale);

public sealed class MarkingSpriteLayerListSerializer :
    ITypeSerializer<List<MarkingSpriteLayer>, SequenceDataNode>,
    ITypeCopier<List<MarkingSpriteLayer>>,
    ITypeCopyCreator<List<MarkingSpriteLayer>>
{
    public List<MarkingSpriteLayer> Read(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<List<MarkingSpriteLayer>>? instanceProvider = null)
    {
        var layers = instanceProvider?.Invoke() ?? new List<MarkingSpriteLayer>();

        foreach (var entry in node.Sequence)
        {
            ReadEntry(serializationManager, entry, hookCtx, context, layers);
        }

        return layers;
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        SequenceDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context)
    {
        var validated = new List<ValidationNode>();

        foreach (var entry in node.Sequence)
        {
            validated.Add(ValidateEntry(serializationManager, entry, context));
        }

        return new ValidatedSequenceNode(validated);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        List<MarkingSpriteLayer> value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        var sequence = new SequenceDataNode();

        foreach (var layer in value)
        {
            var spriteNode = serializationManager.WriteValue(layer.Sprite, alwaysWrite, context, notNullableOverride: true);

            if (!layer.Scale.Equals(Vector2.One) && spriteNode is MappingDataNode mapping)
            {
                mapping.Add("scale", serializationManager.WriteValue(layer.Scale, alwaysWrite, context));
            }

            sequence.Add(spriteNode);
        }

        return sequence;
    }

    public void CopyTo(
        ISerializationManager serializationManager,
        List<MarkingSpriteLayer> source,
        ref List<MarkingSpriteLayer> target,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null)
    {
        target.Clear();
        target.EnsureCapacity(source.Count);

        foreach (var layer in source)
        {
            target.Add(CreateLayerCopy(serializationManager, layer, hookCtx, context));
        }
    }

    public List<MarkingSpriteLayer> CreateCopy(
        ISerializationManager serializationManager,
        List<MarkingSpriteLayer> source,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null)
    {
        var target = new List<MarkingSpriteLayer>(source.Count);

        foreach (var layer in source)
        {
            target.Add(CreateLayerCopy(serializationManager, layer, hookCtx, context));
        }

        return target;
    }

    private static MarkingSpriteLayer CreateLayerCopy(
        ISerializationManager serializationManager,
        MarkingSpriteLayer source,
        SerializationHookContext hookCtx,
        ISerializationContext? context)
    {
        return new MarkingSpriteLayer(
            serializationManager.CreateCopy(source.Sprite, hookCtx, context, notNullableOverride: true),
            source.Scale);
    }

    private static void ReadEntry(
        ISerializationManager serializationManager,
        DataNode entry,
        SerializationHookContext hookCtx,
        ISerializationContext? context,
        List<MarkingSpriteLayer> layers)
    {
        if (entry is not MappingDataNode mapping)
        {
            var sprite = serializationManager.Read<SpriteSpecifier>(entry, hookCtx, context, notNullableOverride: true);
            layers.Add(new MarkingSpriteLayer(sprite, Vector2.One));
            return;
        }

        if (!mapping.TryGet("layers", out SequenceDataNode? groupedLayers))
        {
            var sprite = serializationManager.Read<SpriteSpecifier>(mapping, hookCtx, context, notNullableOverride: true);
            layers.Add(new MarkingSpriteLayer(sprite, ReadScale(serializationManager, mapping, hookCtx, context)));
            return;
        }

        if (!mapping.TryGet("sprite", out var spriteNode))
        {
            throw new InvalidMappingException("Expected sprite-node for grouped marking layers");
        }

        var spritePath = serializationManager.Read<ResPath>(spriteNode, hookCtx, context);

        foreach (var layerNode in groupedLayers.Sequence)
        {
            if (layerNode is not MappingDataNode layerMapping)
            {
                throw new InvalidMappingException("Expected grouped marking layer mapping");
            }

            if (!layerMapping.TryGet("state", out var stateNode) || stateNode is not ValueDataNode stateValue)
            {
                throw new InvalidMappingException("Expected state-node as a valuenode");
            }

            var sprite = new SpriteSpecifier.Rsi(spritePath, stateValue.Value);
            layers.Add(new MarkingSpriteLayer(sprite, ReadScale(serializationManager, layerMapping, hookCtx, context)));
        }
    }

    private static Vector2 ReadScale(
        ISerializationManager serializationManager,
        MappingDataNode mapping,
        SerializationHookContext hookCtx,
        ISerializationContext? context)
    {
        return mapping.TryGet("scale", out var scaleNode)
            ? serializationManager.Read<Vector2>(scaleNode, hookCtx, context)
            : Vector2.One;
    }

    private static ValidationNode ValidateEntry(
        ISerializationManager serializationManager,
        DataNode entry,
        ISerializationContext? context)
    {
        if (entry is not MappingDataNode mapping)
        {
            return serializationManager.ValidateNode<SpriteSpecifier>(entry, context);
        }

        if (!mapping.TryGet("layers", out SequenceDataNode? groupedLayers))
        {
            return ValidateDirectLayer(serializationManager, mapping, context);
        }

        if (!mapping.TryGet("sprite", out var spriteNode))
        {
            return new ErrorNode(mapping, "Grouped marking layer has missing sprite node");
        }

        var validated = new List<ValidationNode>();

        foreach (var layerNode in groupedLayers.Sequence)
        {
            if (layerNode is not MappingDataNode layerMapping)
            {
                validated.Add(new ErrorNode(layerNode, "Grouped marking layer must be a mapping"));
                continue;
            }

            if (!layerMapping.TryGet("state", out var stateNode))
            {
                validated.Add(new ErrorNode(layerMapping, "Grouped marking layer has missing state node"));
                continue;
            }

            var spriteMapping = new MappingDataNode
            {
                { "sprite", spriteNode },
                { "state", stateNode }
            };

            validated.Add(ValidateDirectLayer(serializationManager, spriteMapping, context));

            if (layerMapping.TryGet("scale", out var scaleNode))
            {
                validated.Add(serializationManager.ValidateNode<Vector2>(scaleNode, context));
            }
        }

        return new ValidatedSequenceNode(validated);
    }

    private static ValidationNode ValidateDirectLayer(
        ISerializationManager serializationManager,
        MappingDataNode mapping,
        ISerializationContext? context)
    {
        var validated = new List<ValidationNode>
        {
            serializationManager.ValidateNode<SpriteSpecifier>(mapping, context)
        };

        if (mapping.TryGet("scale", out var scaleNode))
        {
            validated.Add(serializationManager.ValidateNode<Vector2>(scaleNode, context));
        }

        return new ValidatedSequenceNode(validated);
    }
}
