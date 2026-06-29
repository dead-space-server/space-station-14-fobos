using System;
using System.Collections.Generic;
using System.Linq;

namespace Content.Server.Database;

public sealed class UserIdMigrationReport
{
    public UserIdMigrationReport(Guid oldUserId, Guid newUserId)
    {
        OldUserId = oldUserId;
        NewUserId = newUserId;
    }

    public Guid OldUserId { get; }
    public Guid NewUserId { get; }
    public bool Applied { get; set; }
    public List<string> Warnings { get; } = [];
    public List<string> Errors { get; } = [];
    public List<UserIdMigrationTableReport> Tables { get; } = [];
    public bool CanApply => Errors.Count == 0;
    public bool HasOldData => Tables.Any(table => table.OldRows > 0);
}

public sealed record UserIdMigrationTableReport(
    string Name,
    int OldRows,
    int NewRows,
    string Action);
