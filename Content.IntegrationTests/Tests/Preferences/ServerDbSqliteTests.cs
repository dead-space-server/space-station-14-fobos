using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using Content.Server.Administration.Logs;
using Content.Server.Database;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences.Loadouts.Effects;
using Content.Shared.Roles;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Log;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Tests.Preferences
{
    [TestFixture]
    public sealed class ServerDbSqliteTests
    {
        [TestPrototypes]
        private const string Prototypes = @"
- type: dataset
  id: sqlite_test_names_first_male
  values:
  - Aaden

- type: dataset
  id: sqlite_test_names_first_female
  values:
  - Aaliyah

- type: dataset
  id: sqlite_test_names_last_male
  values:
  - Ackerley

- type: dataset
  id: sqlite_test_names_last_female
  values:
  - Ackerla";  // Corvax-LastnameGender

        private static HumanoidCharacterProfile CharlieCharlieson()
        {
            return new()
            {
                Name = "Charlie Charlieson",
                FlavorText = "The biggest boy around.",
                Species = "Human",
                Voice = "Eugene",
                Age = 21,
                Appearance = new(
                    "Afro",
                    Color.Aqua,
                    "Shaved",
                    Color.Aquamarine,
                    Color.Azure,
                    Color.Beige,
                    new ())
            };
        }

        private static ServerDbSqlite GetDb(RobustIntegrationTest.ServerIntegrationInstance server)
        {
            var cfg = server.ResolveDependency<IConfigurationManager>();
            var opsLog = server.ResolveDependency<ILogManager>().GetSawmill("db.ops");
            var builder = new DbContextOptionsBuilder<SqliteServerDbContext>();
            var conn = new SqliteConnection("Data Source=:memory:");
            conn.Open();
            builder.UseSqlite(conn);
            return new ServerDbSqlite(() => builder.Options, true, cfg, true, opsLog);
        }

        [Test]
        public async Task TestUserDoesNotExist()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);
            // Database should be empty so a new GUID should do it.
            Assert.That(await db.GetPlayerPreferencesAsync(NewUserId()), Is.Null);

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestInitPrefs()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);
            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));
            const int slot = 0;
            var originalProfile = CharlieCharlieson();
            await db.InitPrefsAsync(username, originalProfile);
            var prefs = await db.GetPlayerPreferencesAsync(username);
            Assert.That(prefs.Characters.Single(p => p.Key == slot).Value.MemberwiseEquals(originalProfile));
            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestDeleteCharacter()
        {
            var pair = await PoolManager.GetServerClient();
            var server = pair.Server;
            var db = GetDb(server);
            var username = new NetUserId(new Guid("640bd619-fc8d-4fe2-bf3c-4a5fb17d6ddd"));
            await db.InitPrefsAsync(username, new HumanoidCharacterProfile());
            await db.SaveCharacterSlotAsync(username, CharlieCharlieson(), 1);
            await db.SaveSelectedCharacterIndexAsync(username, 1);
            await db.SaveCharacterSlotAsync(username, null, 1);
            var prefs = await db.GetPlayerPreferencesAsync(username);
            Assert.That(!prefs.Characters.Any(p => p.Key != 0));
            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestNoPendingDatabaseChanges()
        {
            var pair = await PoolManager.GetServerClient();
            var server = pair.Server;
            var db = GetDb(server);
            Assert.That(async () => await db.HasPendingModelChanges(), Is.False,
                "The database has pending model changes. Add a new migration to apply them. See https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations");
            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestAutoMapVoteConfigIsPerServer()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);

            var alpha = new AutoMapVoteConfigRecord(
                "alpha",
                true,
                10,
                40,
                80,
                "Packed, Bagel",
                "Omega",
                "Marathon",
                "Core",
                120,
                "Packed",
                "Omega",
                string.Empty,
                "Bagel, Packed",
                "Omega",
                "Marathon");
            var bravo = new AutoMapVoteConfigRecord(
                "bravo",
                false,
                5,
                25,
                50,
                "Box",
                "Meta",
                "Gate",
                string.Empty,
                90,
                string.Empty,
                string.Empty,
                "Gate",
                "Box",
                "Meta",
                "Gate");

            await db.UpsertAutoMapVoteConfigAsync(alpha);
            await db.UpsertAutoMapVoteConfigAsync(bravo);

            var alphaSaved = await db.GetAutoMapVoteConfigAsync("alpha");
            var bravoSaved = await db.GetAutoMapVoteConfigAsync("bravo");

            Assert.Multiple(() =>
            {
                Assert.That(alphaSaved, Is.EqualTo(alpha));
                Assert.That(bravoSaved, Is.EqualTo(bravo));
            });

            var alphaUpdated = alpha with
            {
                Enabled = false,
                SmallMaps = "Aspid",
                VoteDurationSeconds = 45
            };

            await db.UpsertAutoMapVoteConfigAsync(alphaUpdated);
            var alphaAfterUpdate = await db.GetAutoMapVoteConfigAsync("alpha");
            var bravoAfterUpdate = await db.GetAutoMapVoteConfigAsync("bravo");

            Assert.Multiple(() =>
            {
                Assert.That(alphaAfterUpdate, Is.EqualTo(alphaUpdated));
                Assert.That(bravoAfterUpdate, Is.EqualTo(bravo));
            });

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestUserIdMigrationMergesCoreRows()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);

            var oldUser = new NetUserId(new Guid("a07b73e3-7865-4ef4-8ad8-d9bb70335a10"));
            var newUser = new NetUserId(new Guid("2b398e34-f45b-43e3-9492-2c949258b0bf"));

            await db.UpdatePlayerRecord(oldUser, "OldUser", IPAddress.Loopback, null);
            await db.UpdatePlayerRecord(newUser, "NewUser", IPAddress.Parse("127.0.0.2"), null);
            await db.InitPrefsAsync(oldUser, CharlieCharlieson());
            await db.SaveCharacterSlotAsync(oldUser, CharlieCharlieson(), 1);
            await db.InitPrefsAsync(newUser, new HumanoidCharacterProfile());
            var (server, _) = await db.AddOrGetServer("migration-test");
            var roundId = await db.AddNewRound(server, oldUser.UserId, newUser.UserId);
            await db.AddAdminLogs([
                new AdminLog
                {
                    Id = 1,
                    RoundId = roundId,
                    Type = LogType.Unknown,
                    Impact = LogImpact.Low,
                    Date = DateTime.UtcNow,
                    Message = "migration log",
                    Json = JsonSerializer.SerializeToDocument(new Dictionary<string, string>()),
                    Players =
                    [
                        new AdminLogPlayer { RoundId = roundId, LogId = 1, PlayerUserId = oldUser.UserId },
                        new AdminLogPlayer { RoundId = roundId, LogId = 1, PlayerUserId = newUser.UserId },
                    ],
                },
            ]);
            await db.AddToWhitelistAsync(oldUser);
            await db.AddAdminAsync(new Admin
            {
                UserId = oldUser.UserId,
                Deadminned = true,
                Suspended = true,
                Flags =
                [
                    new AdminFlag { Flag = "Ban" },
                ],
            }, CancellationToken.None);
            await db.AddAdminAsync(new Admin
            {
                UserId = newUser.UserId,
                Deadminned = false,
                Suspended = false,
                Flags =
                [
                    new AdminFlag { Flag = "Admin" },
                ],
            }, CancellationToken.None);

            var job = new ProtoId<JobPrototype>("Captain");
            Assert.That(await db.AddJobWhitelist(oldUser.UserId, job), Is.True);
            var noteId = await db.AddAdminNote(new AdminNote
            {
                PlayerUserId = oldUser.UserId,
                CreatedById = oldUser.UserId,
                LastEditedById = oldUser.UserId,
                PlaytimeAtNote = TimeSpan.FromMinutes(3),
                Message = "old note",
                Severity = NoteSeverity.Medium,
                CreatedAt = DateTime.UtcNow,
                LastEditedAt = DateTime.UtcNow,
            });
            var ban = await db.AddBanAsync(new BanDef(
                null,
                BanType.Server,
                ImmutableArray.Create(oldUser),
                ImmutableArray<(IPAddress address, int cidrMask)>.Empty,
                ImmutableArray<ImmutableTypedHwid>.Empty,
                DateTimeOffset.UtcNow,
                null,
                ImmutableArray<int>.Empty,
                TimeSpan.FromMinutes(3),
                "old ban",
                NoteSeverity.Minor,
                oldUser,
                null));
            await db.UpdatePlayTimes([
                new PlayTimeUpdate(oldUser, "Overall", TimeSpan.FromMinutes(10)),
                new PlayTimeUpdate(newUser, "Overall", TimeSpan.FromMinutes(5)),
            ]);

            var dryRun = await db.DryRunUserIdMigrationAsync(oldUser.UserId, newUser.UserId);
            Assert.That(dryRun.CanApply, Is.True);
            Assert.That(dryRun.HasOldData, Is.True);

            var applied = await db.ApplyUserIdMigrationAsync(oldUser.UserId, newUser.UserId);
            Assert.That(applied.Applied, Is.True);

            Assert.That(await db.GetPlayerRecordByUserId(oldUser, CancellationToken.None), Is.Null);
            Assert.That(await db.GetPlayerRecordByUserId(newUser, CancellationToken.None), Is.Not.Null);
            Assert.That(await db.GetPlayerPreferencesAsync(oldUser), Is.Null);
            var migratedPrefs = (await db.GetPlayerPreferencesAsync(newUser))!;
            Assert.That(migratedPrefs.Characters, Has.Count.EqualTo(3));
            Assert.That(migratedPrefs.Characters.Keys, Is.EquivalentTo(new[] { 0, 1, 2 }));
            Assert.That(migratedPrefs.SelectedCharacterIndex, Is.EqualTo(0));
            Assert.That(await db.GetWhitelistStatusAsync(oldUser), Is.False);
            Assert.That(await db.GetWhitelistStatusAsync(newUser), Is.True);

            var migratedAdmin = await db.GetAdminDataForAsync(newUser, CancellationToken.None);
            Assert.That(await db.GetAdminDataForAsync(oldUser, CancellationToken.None), Is.Null);
            Assert.That(migratedAdmin, Is.Not.Null);
            Assert.That(migratedAdmin!.Suspended, Is.True);
            Assert.That(migratedAdmin.Deadminned, Is.False);
            Assert.That(migratedAdmin.Flags.Select(flag => flag.Flag), Is.EquivalentTo(new[] { "Admin", "Ban" }));

            Assert.That(await db.GetJobWhitelists(oldUser.UserId, CancellationToken.None), Is.Empty);
            Assert.That(await db.GetJobWhitelists(newUser.UserId, CancellationToken.None), Has.One.EqualTo("Captain"));

            var migratedNote = await db.GetAdminNote(noteId);
            Assert.That(migratedNote, Is.Not.Null);
            Assert.That(migratedNote!.Player!.UserId, Is.EqualTo(newUser));
            Assert.That(migratedNote.CreatedBy!.UserId, Is.EqualTo(newUser));
            Assert.That(migratedNote.LastEditedBy!.UserId, Is.EqualTo(newUser));

            var migratedBan = await db.GetBanAsync(ban.Id!.Value);
            Assert.That(migratedBan, Is.Not.Null);
            Assert.That(migratedBan!.UserIds, Does.Contain(newUser));
            Assert.That(migratedBan.UserIds, Does.Not.Contain(oldUser));
            Assert.That(migratedBan.BanningAdmin, Is.EqualTo(newUser));

            var migratedRound = await db.GetRound(roundId);
            Assert.That(migratedRound.Players.Select(player => player.UserId), Is.EquivalentTo(new[] { newUser.UserId }));

            var migratedLogs = new List<SharedAdminLog>();
            await foreach (var log in db.GetAdminLogs(new LogFilter { Round = roundId }))
                migratedLogs.Add(log);

            Assert.That(migratedLogs, Has.One.Items);
            Assert.That(migratedLogs.Single().Players, Is.EquivalentTo(new[] { newUser.UserId }));

            var playTimes = await db.GetPlayTimes(newUser.UserId, CancellationToken.None);
            Assert.That(playTimes.Single(p => p.Tracker == "Overall").TimeSpent, Is.EqualTo(TimeSpan.FromMinutes(15)));
            Assert.That(await db.GetPlayTimes(oldUser.UserId, CancellationToken.None), Is.Empty);

            var secondApply = await db.ApplyUserIdMigrationAsync(oldUser.UserId, newUser.UserId);
            Assert.That(secondApply.Applied, Is.False);
            Assert.That(secondApply.HasOldData, Is.False);

            var playTimesAfterSecondApply = await db.GetPlayTimes(newUser.UserId, CancellationToken.None);
            Assert.That(playTimesAfterSecondApply.Single(p => p.Tracker == "Overall").TimeSpent, Is.EqualTo(TimeSpan.FromMinutes(15)));

            var partialOldUser = new NetUserId(new Guid("b2f06265-adcd-4300-a815-75287d39e986"));
            var partialNewUser = new NetUserId(new Guid("3800ee9f-5189-4519-b8ee-bf8b0d931d63"));
            await db.UpdatePlayerRecord(partialOldUser, "PartialOld", IPAddress.Loopback, null);
            await db.UpdatePlayerRecord(partialNewUser, "PartialNew", IPAddress.Loopback, null);

            var partialApply = await db.ApplyUserIdMigrationAsync(partialOldUser.UserId, partialNewUser.UserId);
            Assert.That(partialApply.Applied, Is.True);
            Assert.That(await db.GetPlayerRecordByUserId(partialOldUser, CancellationToken.None), Is.Null);
            Assert.That(await db.GetPlayerRecordByUserId(partialNewUser, CancellationToken.None), Is.Not.Null);
            Assert.That(await db.GetPlayerPreferencesAsync(partialNewUser), Is.Null);
            Assert.That(await db.GetPlayTimes(partialNewUser.UserId, CancellationToken.None), Is.Empty);
            Assert.That(await db.GetAdminDataForAsync(partialNewUser, CancellationToken.None), Is.Null);

            var emptyApply = await db.ApplyUserIdMigrationAsync(new Guid("d7dfca87-d7a4-4a21-b0eb-4a233a721255"), partialNewUser.UserId);
            Assert.That(emptyApply.Applied, Is.False);
            Assert.That(emptyApply.HasOldData, Is.False);

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task TestUserIdLoginMigrationSkipsHistoricalRows()
        {
            var pair = await PoolManager.GetServerClient();
            var db = GetDb(pair.Server);

            var oldUser = new NetUserId(new Guid("c91bb5c1-7240-4f4b-9f8b-ef35ca7563a8"));
            var newUser = new NetUserId(new Guid("8145a12f-ac2b-4e98-88c9-361c02da6c2a"));

            await db.UpdatePlayerRecord(oldUser, "OldLoginUser", IPAddress.Loopback, null);
            await db.UpdatePlayerRecord(newUser, "NewLoginUser", IPAddress.Parse("127.0.0.3"), null);
            await db.InitPrefsAsync(oldUser, CharlieCharlieson());
            await db.AddToWhitelistAsync(oldUser);
            await db.AddAdminAsync(new Admin
            {
                UserId = oldUser.UserId,
                Suspended = true,
                Flags =
                [
                    new AdminFlag { Flag = "Ban" },
                ],
            }, CancellationToken.None);

            var job = new ProtoId<JobPrototype>("Captain");
            Assert.That(await db.AddJobWhitelist(oldUser.UserId, job), Is.True);
            await db.UpdatePlayTimes([
                new PlayTimeUpdate(oldUser, "Overall", TimeSpan.FromMinutes(7)),
            ]);

            var (server, _) = await db.AddOrGetServer("login-migration-test");
            var roundId = await db.AddNewRound(server, oldUser.UserId);
            var noteId = await db.AddAdminNote(new AdminNote
            {
                PlayerUserId = oldUser.UserId,
                CreatedById = oldUser.UserId,
                PlaytimeAtNote = TimeSpan.FromMinutes(3),
                Message = "historical note",
                Severity = NoteSeverity.Medium,
                CreatedAt = DateTime.UtcNow,
                LastEditedAt = DateTime.UtcNow,
            });

            var applied = await db.ApplyUserIdLoginMigrationAsync(oldUser.UserId, newUser.UserId);
            Assert.That(applied.Applied, Is.True);
            Assert.That(applied.Tables.Select(table => table.Name), Does.Not.Contain("connection_log"));
            Assert.That(applied.Tables.Select(table => table.Name), Does.Not.Contain("admin_notes"));
            Assert.That(applied.Tables.Select(table => table.Name), Does.Not.Contain("player_round"));

            Assert.That(await db.GetPlayerRecordByUserId(oldUser, CancellationToken.None), Is.Not.Null);
            Assert.That(await db.GetPlayerRecordByUserId(newUser, CancellationToken.None), Is.Not.Null);
            Assert.That(await db.GetPlayerPreferencesAsync(oldUser), Is.Null);
            Assert.That(await db.GetPlayerPreferencesAsync(newUser), Is.Not.Null);
            Assert.That(await db.GetWhitelistStatusAsync(oldUser), Is.False);
            Assert.That(await db.GetWhitelistStatusAsync(newUser), Is.True);
            Assert.That(await db.GetAdminDataForAsync(oldUser, CancellationToken.None), Is.Null);
            Assert.That(await db.GetAdminDataForAsync(newUser, CancellationToken.None), Is.Not.Null);
            Assert.That(await db.GetJobWhitelists(oldUser.UserId, CancellationToken.None), Is.Empty);
            Assert.That(await db.GetJobWhitelists(newUser.UserId, CancellationToken.None), Has.One.EqualTo("Captain"));
            Assert.That((await db.GetPlayTimes(newUser.UserId, CancellationToken.None)).Single().TimeSpent, Is.EqualTo(TimeSpan.FromMinutes(7)));
            Assert.That(await db.GetPlayTimes(oldUser.UserId, CancellationToken.None), Is.Empty);

            var historicalNote = await db.GetAdminNote(noteId);
            Assert.That(historicalNote!.Player!.UserId, Is.EqualTo(oldUser));
            Assert.That(historicalNote.CreatedBy!.UserId, Is.EqualTo(oldUser));
            var historicalRound = await db.GetRound(roundId);
            Assert.That(historicalRound.Players.Select(player => player.UserId), Is.EquivalentTo(new[] { oldUser.UserId }));

            var secondApply = await db.ApplyUserIdLoginMigrationAsync(oldUser.UserId, newUser.UserId);
            Assert.That(secondApply.Applied, Is.False);
            Assert.That(secondApply.HasOldData, Is.False);

            await pair.CleanReturnAsync();
        }

        private static NetUserId NewUserId()
        {
            return new(Guid.NewGuid());
        }
    }
}
