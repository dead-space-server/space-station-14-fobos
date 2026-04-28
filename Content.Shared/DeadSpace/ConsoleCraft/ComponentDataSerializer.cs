// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.IoC;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared.DeadSpace.ConsoleCraft;

[TypeSerializer]
public sealed class ComponentDataSerializer
    : ITypeSerializer<Dictionary<string, MappingDataNode>, MappingDataNode>
{
    public Dictionary<string, MappingDataNode> Read(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        SerializationHookContext hookCtx,
        ISerializationContext? context = null,
        ISerializationManager.InstantiationDelegate<Dictionary<string, MappingDataNode>>? instanceProvider = null)
    {
        var result = new Dictionary<string, MappingDataNode>();
        foreach (var (key, valueNode) in node)
        {
            result[key] = valueNode is MappingDataNode mapping
                ? mapping
                : new MappingDataNode();
        }
        return result;
    }

    public ValidationNode Validate(
        ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        return new ValidatedValueNode(node);
    }

    public DataNode Write(
        ISerializationManager serializationManager,
        Dictionary<string, MappingDataNode> value,
        IDependencyCollection dependencies,
        bool alwaysWrite = false,
        ISerializationContext? context = null)
    {
        var mapping = new MappingDataNode();
        foreach (var (key, val) in value)
            mapping.Add(key, val);
        return mapping;
    }

    public Dictionary<string, MappingDataNode> Copy(
        ISerializationManager serializationManager,
        Dictionary<string, MappingDataNode> source,
        Dictionary<string, MappingDataNode> target,
        bool skipHook,
        ISerializationContext? context = null)
    {
        target.Clear();
        foreach (var (k, v) in source)
            target[k] = v;
        return target;
    }
}
