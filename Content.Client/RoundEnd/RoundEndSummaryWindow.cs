using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Shared.GameTicking;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;

            MinSize = SetSize = new Vector2(920, 720); // DS14-resize

            Title = Loc.GetString("round-end-summary-window-title");

            // The round end window is split into two tabs, one about the round stats
            // and the other is a list of RoundEndPlayerInfo for each player.
            // This tab would be a good place for things like: "x many people died.",
            // "clown slipped the crew x times.", "x shots were fired this round.", etc.
            // Also good for serious info.

            RoundId = roundId;
            var roundEndTabs = new TabContainer();
            roundEndTabs.AddChild(MakeRoundEndSummaryTab(gm, roundEnd, roundTimeSpan, roundId, info));
            roundEndTabs.AddChild(MakePlayerManifestTab(info));

            ContentsContainer.AddChild(roundEndTabs);

            OpenCenteredRight();
            MoveToFront();
        }

        private BoxContainer MakeRoundEndSummaryTab(string gamemode, string roundEnd, TimeSpan roundDuration, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var roundEndSummaryTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-round-end-summary-tab-title")
            };

            // DS14-start
            var background = new PanelContainer
            {
                StyleClasses = { "BackgroundPanelDark" },
                HorizontalExpand = true,
                VerticalExpand = true,
            };
            // DS14-end

            var roundEndSummaryContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                HorizontalExpand = true, // DS14
                Margin = new Thickness(10)
            };
            var roundEndSummaryContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 10, // DS14
                HorizontalExpand = true, // DS14
            };

            // DS14-start
            roundEndSummaryContainer.AddChild(MakeRoundOverviewCard(gamemode, roundDuration, roundId));

            var roundOutcome = GetRoundOutcomeSummary(roundEnd);
            if (roundOutcome != null)
                roundEndSummaryContainer.AddChild(MakeRoundOutcomeCard(roundOutcome));

            roundEndSummaryContainer.AddChild(MakeAntagManifestSection(playersInfo));
            // DS14-end

            roundEndSummaryContainerScrollbox.AddChild(roundEndSummaryContainer);
            background.AddChild(roundEndSummaryContainerScrollbox); // DS14
            roundEndSummaryTab.AddChild(background); // DS14

            return roundEndSummaryTab;
        }

        private BoxContainer MakePlayerManifestTab(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var playerManifestTab = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Name = Loc.GetString("round-end-summary-window-player-manifest-tab-title")
            };

            // DS14-start
            var background = new PanelContainer
            {
                StyleClasses = { "BackgroundPanelDark" },
                HorizontalExpand = true,
                VerticalExpand = true,
            };
            // DS14-end

            var playerInfoContainerScrollbox = new ScrollContainer
            {
                VerticalExpand = true,
                HorizontalExpand = true, // DS14
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8, // DS14
                HorizontalExpand = true, // DS14
            };

            //Put observers at the bottom of the list. Put antags on top.
            var sortedPlayersInfo = playersInfo.OrderBy(p => p.Observer).ThenBy(p => !p.Antag).ToArray();
            playerInfoContainer.AddChild(MakePlayerManifestHeader(sortedPlayersInfo.Length)); // DS14

            foreach (var playerInfo in sortedPlayersInfo)
            {
                playerInfoContainer.AddChild(MakePlayerManifestCard(playerInfo));
            }

            playerInfoContainerScrollbox.AddChild(playerInfoContainer);
            background.AddChild(playerInfoContainerScrollbox);
            playerManifestTab.AddChild(background);
            // DS14-end

            return playerManifestTab;
        }

        // DS14-start
        private static readonly Color ManifestBodyBackground = Color.FromHex("#0d1117");
        private static readonly Color ManifestPanelBorder = Color.FromHex("#30363d");
        private static readonly Color ObjectiveSuccessColor = Color.FromHex("#3fb950");
        private static readonly Color ObjectivePartialSuccessColor = Color.FromHex("#d29922");
        private static readonly Color ObjectivePartialFailureColor = Color.FromHex("#f0883e");
        private static readonly Color ObjectiveFailureColor = Color.FromHex("#f85149");

        private Control MakeRoundOverviewCard(string gamemode, TimeSpan roundDuration, int roundId)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var box = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(10),
                SeparationOverride = 4,
                HorizontalExpand = true,
            };

            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-round-card-title", ("roundId", roundId)),
                StyleClasses = { "LabelBig" },
                HorizontalExpand = true,
            });
            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-round-card-gamemode", ("gamemode", gamemode)),
                StyleClasses = { "LabelSubText" },
                HorizontalExpand = true,
            });
            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-round-card-duration",
                    ("hours", roundDuration.Hours),
                    ("minutes", roundDuration.Minutes),
                    ("seconds", roundDuration.Seconds)),
                StyleClasses = { "LabelSubText" },
                HorizontalExpand = true,
            });

            panel.AddChild(box);
            return panel;
        }

        private static Control MakeRoundOutcomeCard(string roundOutcome)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var box = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(10),
                SeparationOverride = 4,
                HorizontalExpand = true,
            };

            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-result-card-title"),
                StyleClasses = { "LabelHeading" },
                HorizontalExpand = true,
            });

            var outcomeLabel = new RichTextLabel
            {
                HorizontalExpand = true,
                MinHeight = 34,
            };
            outcomeLabel.SetMarkup($"[font size=18]{roundOutcome}[/font]");
            box.AddChild(outcomeLabel);

            panel.AddChild(box);
            return panel;
        }

        private Control MakePlayerManifestHeader(int playerCount)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var box = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(10),
                SeparationOverride = 4,
                HorizontalExpand = true,
            };

            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-player-manifest-tab-title"),
                StyleClasses = { "LabelBig" },
                HorizontalExpand = true,
            });
            box.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-player-manifest-subtitle", ("count", playerCount)),
                StyleClasses = { "LabelSubText" },
                HorizontalExpand = true,
            });

            panel.AddChild(box);
            return panel;
        }

        private Control MakePlayerManifestCard(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var card = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Margin = new Thickness(10),
                SeparationOverride = 10,
                HorizontalExpand = true,
            };

            card.AddChild(MakePlayerManifestDoll(playerInfo));

            var content = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 5,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            content.AddChild(new Label
            {
                Text = playerInfo.PlayerICName ?? Loc.GetString("generic-unknown-title"),
                StyleClasses = { playerInfo.Antag ? "LabelBig" : "LabelHeading" },
                ClipText = true,
                HorizontalExpand = true,
            });

            var roleText = GetPlayerManifestRoleText(playerInfo);

            content.AddChild(MakeDetailRow(
                Loc.GetString("round-end-summary-window-player-manifest-ooc",
                    ("playerOOCName", playerInfo.PlayerOOCName)),
                roleText));
            content.AddChild(MakeQuoteLabel(playerInfo.ManifestQuote));

            card.AddChild(content);
            panel.AddChild(card);
            return panel;
        }

        private Control MakePlayerManifestDoll(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var panel = new PanelContainer
            {
                SetSize = new Vector2(60, 60),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = ManifestBodyBackground,
                    BorderColor = ManifestPanelBorder,
                    BorderThickness = new Thickness(1),
                },
            };

            if (playerInfo.PlayerNetEntity != null)
            {
                panel.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                {
                    OverrideDirection = Direction.South,
                    SetSize = new Vector2(58, 58),
                    Scale = new Vector2(1.45f, 1.45f),
                    HorizontalAlignment = HAlignment.Center,
                    VerticalAlignment = VAlignment.Center,
                });

                return panel;
            }

            panel.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-no-body"),
                Align = Label.AlignMode.Center,
                StyleClasses = { "LabelSubText" },
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
            });
            return panel;
        }

        private Control MakeAntagManifestSection(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var antags = playersInfo
                .Where(player => player.Antag)
                .OrderByDescending(player => player.ManifestKills)
                .ThenByDescending(player => player.ManifestAssists)
                .ThenBy(GetRoleSortKey)
                .ThenBy(player => player.PlayerOOCName)
                .ToArray();

            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Margin = new Thickness(0, 10, 0, 0),
                SeparationOverride = 10,
                HorizontalExpand = true,
            };

            root.AddChild(MakeManifestHeader(antags.Length));

            if (antags.Length == 0)
            {
                root.AddChild(MakeEmptyManifestPanel());
                return root;
            }

            foreach (var playerInfo in antags)
            {
                root.AddChild(MakeAntagManifestCard(playerInfo));
            }

            return root;
        }

        private Control MakeManifestHeader(int antagCount)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var box = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Margin = new Thickness(10),
                SeparationOverride = 10,
                HorizontalExpand = true,
            };

            var labels = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
            };
            labels.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-title"),
                StyleClasses = { "LabelBig" },
                HorizontalExpand = true,
            });
            labels.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-subtitle", ("count", antagCount)),
                StyleClasses = { "LabelSubText" },
                HorizontalExpand = true,
            });

            box.AddChild(labels);
            panel.AddChild(box);
            return panel;
        }

        private Control MakeAntagManifestCard(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var card = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Margin = new Thickness(10),
                SeparationOverride = 12,
                HorizontalExpand = true,
            };

            card.AddChild(MakeAntagDoll(playerInfo));

            var content = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 6,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            content.AddChild(new Label
            {
                Text = playerInfo.PlayerICName ?? Loc.GetString("generic-unknown-title"),
                StyleClasses = { "LabelBig" },
                ClipText = true,
                HorizontalExpand = true,
            });

            content.AddChild(MakeDetailRow(
                Loc.GetString("round-end-summary-window-antag-manifest-ooc",
                    ("playerOOCName", playerInfo.PlayerOOCName)),
                Loc.GetString("round-end-summary-window-antag-manifest-role", ("roles", GetAntagRolesText(playerInfo)))));

            content.AddChild(MakeDetailRow(
                Loc.GetString("round-end-summary-window-antag-manifest-kills", ("kills", playerInfo.ManifestKills)),
                Loc.GetString("round-end-summary-window-antag-manifest-assists", ("assists", playerInfo.ManifestAssists))));

            content.AddChild(MakeQuoteLabel(playerInfo.ManifestQuote));

            var objectives = playerInfo.ManifestObjectives ?? Array.Empty<RoundEndMessageEvent.RoundEndObjectiveInfo>();
            if (objectives.Length > 0)
                content.AddChild(MakeObjectivesTable(objectives));

            card.AddChild(content);
            panel.AddChild(card);
            return panel;
        }

        private static Control MakeDetailRow(string left, string right)
        {
            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 16,
                HorizontalExpand = true,
            };

            row.AddChild(MakeDetailLabel(left));
            row.AddChild(MakeDetailLabel(right));
            return row;
        }

        private static Label MakeDetailLabel(string text)
        {
            return new Label
            {
                Text = text,
                StyleClasses = { "LabelSubText" },
                ClipText = true,
                HorizontalExpand = true,
            };
        }

        private static Control MakeQuoteLabel(string quote)
        {
            var displayQuote = string.IsNullOrWhiteSpace(quote)
                ? Loc.GetString("round-end-summary-window-antag-manifest-quote-fallback")
                : quote;

            var quoteLabel = new RichTextLabel
            {
                HorizontalExpand = true,
                VerticalExpand = true,
                MinHeight = 46,
            };
            quoteLabel.SetMarkup(Loc.GetString("round-end-summary-window-antag-manifest-quote",
                ("quote", FormattedMessage.EscapeText(displayQuote))));
            return quoteLabel;
        }

        private static Control MakeObjectivesTable(RoundEndMessageEvent.RoundEndObjectiveInfo[] objectives)
        {
            var table = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 4,
                HorizontalExpand = true,
                Margin = new Thickness(0, 4, 0, 0),
            };

            table.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-objectives-table-title"),
                StyleClasses = { "LabelHeading" },
                HorizontalExpand = true,
            });

            table.AddChild(MakeObjectivesTableRow(
                Loc.GetString("round-end-summary-window-objectives-table-objective"),
                Loc.GetString("round-end-summary-window-objectives-table-status"),
                header: true));

            foreach (var objective in objectives)
            {
                var (status, color) = GetObjectiveStatus(objective.Progress);
                table.AddChild(MakeObjectivesTableRow(objective.Title, status, color));
            }

            return table;
        }

        private static Control MakeObjectivesTableRow(string objective, string status, Color? statusColor = null, bool header = false)
        {
            var row = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 12,
                HorizontalExpand = true,
            };

            var objectiveLabel = new RichTextLabel
            {
                HorizontalExpand = true,
                SizeFlagsStretchRatio = 2.4f,
            };
            objectiveLabel.SetMarkup(header
                ? $"[bold]{FormattedMessage.EscapeText(objective)}[/bold]"
                : FormattedMessage.EscapeText(objective));

            var statusLabel = new Label
            {
                Text = status,
                StyleClasses = { header ? "LabelSubText" : "Label" },
                ClipText = true,
                HorizontalExpand = true,
                SizeFlagsStretchRatio = 1f,
            };

            if (statusColor != null)
                statusLabel.FontColorOverride = statusColor.Value;

            row.AddChild(objectiveLabel);
            row.AddChild(statusLabel);
            return row;
        }

        private static (string Status, Color Color) GetObjectiveStatus(float progress)
        {
            var clampedProgress = Math.Clamp(progress, 0f, 1f);
            var progressPercent = Math.Round(clampedProgress * 100f);

            if (clampedProgress > 0.99f)
            {
                return (Loc.GetString("round-end-summary-window-objectives-status-success",
                    ("progress", progressPercent)), ObjectiveSuccessColor);
            }

            if (clampedProgress >= 0.5f)
            {
                return (Loc.GetString("round-end-summary-window-objectives-status-partial-success",
                    ("progress", progressPercent)), ObjectivePartialSuccessColor);
            }

            if (clampedProgress > 0f)
            {
                return (Loc.GetString("round-end-summary-window-objectives-status-partial-failure",
                    ("progress", progressPercent)), ObjectivePartialFailureColor);
            }

            return (Loc.GetString("round-end-summary-window-objectives-status-failure",
                ("progress", progressPercent)), ObjectiveFailureColor);
        }

        private Control MakeAntagDoll(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var panel = new PanelContainer
            {
                SetSize = new Vector2(160, 160),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = ManifestBodyBackground,
                    BorderColor = ManifestPanelBorder,
                    BorderThickness = new Thickness(1),
                },
            };

            if (playerInfo.PlayerNetEntity != null)
            {
                panel.AddChild(new SpriteView(playerInfo.PlayerNetEntity.Value, _entityManager)
                {
                    OverrideDirection = Direction.South,
                    SetSize = new Vector2(160, 160),
                    Scale = new Vector2(3f, 3f),
                    HorizontalAlignment = HAlignment.Center,
                    VerticalAlignment = VAlignment.Center,
                });

                return panel;
            }

            panel.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-no-body"),
                Align = Label.AlignMode.Center,
                StyleClasses = { "LabelSubText" },
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
            });
            return panel;
        }

        private static Control MakeEmptyManifestPanel()
        {
            var panel = new PanelContainer
            {
                StyleClasses = { "PanelDark" },
                HorizontalExpand = true,
            };

            var box = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Margin = new Thickness(10),
                SeparationOverride = 10,
                HorizontalExpand = true,
            };

            var labels = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
            };
            labels.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-empty-title"),
                StyleClasses = { "LabelHeading" },
                HorizontalExpand = true,
            });
            labels.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-antag-manifest-empty-subtitle"),
                StyleClasses = { "LabelSubText" },
                HorizontalExpand = true,
            });

            box.AddChild(labels);
            panel.AddChild(box);
            return panel;
        }

        private static string GetAntagRolesText(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var roles = playerInfo.AntagRoleNames ?? Array.Empty<string>();
            if (roles.Length == 0 && !string.IsNullOrWhiteSpace(playerInfo.Role))
                roles = new[] { playerInfo.Role };

            if (roles.Length == 0)
                return Loc.GetString("game-ticker-unknown-role");

            return string.Join(", ", roles.Select(LocalizeOrRaw));
        }

        private static string GetJobRolesText(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var roles = playerInfo.JobRoleNames ?? Array.Empty<string>();
            if (roles.Length == 0 && !string.IsNullOrWhiteSpace(playerInfo.Role))
                roles = new[] { playerInfo.Role };

            if (roles.Length == 0)
                return Loc.GetString("game-ticker-unknown-role");

            return string.Join(", ", roles.Select(LocalizeOrRaw));
        }

        private static string GetPlayerManifestRoleText(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            if (playerInfo.Observer)
                return Loc.GetString("round-end-summary-window-player-manifest-observer");

            var hasStationRole = (playerInfo.JobRoleNames ?? Array.Empty<string>()).Length > 0;
            var jobRoles = GetJobRolesText(playerInfo);
            var antagRoles = playerInfo.AntagRoleNames ?? Array.Empty<string>();
            if (hasStationRole && antagRoles.Length > 0)
            {
                return Loc.GetString("round-end-summary-window-player-manifest-role-with-antagonist",
                    ("role", jobRoles),
                    ("antagonist", GetAntagRolesText(playerInfo)));
            }

            return Loc.GetString("round-end-summary-window-player-manifest-role", ("role", jobRoles));
        }

        private static string? GetRoundOutcomeSummary(string roundEnd)
        {
            if (string.IsNullOrWhiteSpace(roundEnd))
                return null;

            var lines = roundEnd
                .Replace("\r\n", "\n")
                .Replace('\r', '\n')
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    return trimmed;
            }

            return null;
        }

        private static string GetRoleSortKey(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            var roles = playerInfo.AntagRoleNames ?? Array.Empty<string>();
            if (roles.Length > 0)
                return roles[0];

            return playerInfo.Role ?? string.Empty;
        }

        private static string LocalizeOrRaw(string value)
        {
            return Loc.TryGetString(value, out var localized) ? localized : value;
        }
        // DS14-end
    }

}
