using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> UserIdMigrationAutoEnabled =
        CVarDef.Create("user_id_migration.auto_enabled", false, CVar.SERVERONLY);

    /// <summary>
    /// Auth endpoint used to resolve the linked WizDen UUID for an MK UUID.
    /// May be an absolute URL or a path relative to <c>auth.server</c>.
    /// If the value does not contain <c>{mkUserId}</c> or <c>{0}</c>, the MK UUID is appended as a <c>mkUserId</c> query parameter.
    /// </summary>
    public static readonly CVarDef<string> UserIdMigrationAuthEndpoint =
        CVarDef.Create("user_id_migration.auth_endpoint", "", CVar.SERVERONLY);

    public static readonly CVarDef<int> UserIdMigrationAuthTimeoutSeconds =
        CVarDef.Create("user_id_migration.auth_timeout_seconds", 5, CVar.SERVERONLY);
}
