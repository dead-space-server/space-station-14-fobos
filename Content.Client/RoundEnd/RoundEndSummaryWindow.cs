using System.Linq;
using System.Numerics;
using Content.Client.Message;
using Content.Client.UserInterface.Controls;
using Content.Shared.Chat.TypingIndicator;
using Content.Shared.GameTicking;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.RoundEnd
{
    public sealed class RoundEndSummaryWindow : DefaultWindow
    {
        private readonly IEntityManager _entityManager;
        private readonly List<RoundEndManifestDollView> _manifestDollViews = new();
        public int RoundId;

        public RoundEndSummaryWindow(string gm, string roundEnd, TimeSpan roundTimeSpan, int roundId,
            RoundEndMessageEvent.RoundEndPlayerInfo[] info, IEntityManager entityManager)
        {
            _entityManager = entityManager;

            // DS14-start
            var initialWindowSize = GetInitialWindowSize(info);
            MinSize = DefaultWindowSize;
            SetSize = initialWindowSize;
            // DS14-end

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
                HScrollEnabled = false, // DS14
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
                HScrollEnabled = false, // DS14
                Margin = new Thickness(10)
            };
            var playerInfoContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 8, // DS14
                HorizontalExpand = true, // DS14
            };

            // DS14-start
            var sortedPlayersInfo = playersInfo
                .OrderBy(player => string.IsNullOrWhiteSpace(player.PlayerICName))
                .ThenBy(player => player.PlayerICName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(player => player.PlayerOOCName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
            playerInfoContainer.AddChild(MakePlayerManifestHeader(sortedPlayersInfo.Length));
            // DS14-end

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
        private static readonly Color ManifestCardSeparatorColor = Color.FromHex("#30363d");
        private static readonly Vector2 DefaultWindowSize = new(920, 720);
        private const float RoundEndSummaryWindowHorizontalPadding = 160f;
        private const float AntagManifestCardHorizontalPadding = 20f;
        private const float AntagManifestDollWidth = 160f;
        private const float AntagManifestCardSeparation = 12f;
        private const float PlayerManifestDollWidth = 60f;
        private const float PlayerManifestCardSeparation = 10f;
        private const float ApproximateSubTextGlyphWidth = 8f;
        private const int DetailRowSeparation = 16;
        private const string ManifestFallbackPrototype = "MobObserver";

        private bool? _fallbackDollDrawable;

        private Vector2 GetInitialWindowSize(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var width = MathF.Max(DefaultWindowSize.X, GetAntagManifestRequiredWindowWidth(playersInfo));
            width = MathF.Max(width, GetPlayerManifestRequiredWindowWidth(playersInfo));
            return DefaultWindowSize with { X = width };
        }

        private float GetAntagManifestRequiredWindowWidth(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var maxCardWidth = 0f;

            foreach (var playerInfo in playersInfo.Where(player => player.ShowInAntagManifest))
            {
                var detailRowWidth = MeasureDetailRowWidth(
                    Loc.GetString("round-end-summary-window-antag-manifest-ooc",
                        ("playerOOCName", playerInfo.PlayerOOCName)),
                    GetPlayerManifestRoleText(playerInfo));

                detailRowWidth = MathF.Max(detailRowWidth, MeasureDetailRowWidth(
                    Loc.GetString("round-end-summary-window-antag-manifest-kills", ("kills", playerInfo.ManifestKills)),
                    Loc.GetString("round-end-summary-window-antag-manifest-assists", ("assists", playerInfo.ManifestAssists))));

                var dollWidth = HasDrawableManifestDoll(playerInfo.PlayerNetEntity)
                    ? AntagManifestDollWidth + AntagManifestCardSeparation
                    : 0f;

                maxCardWidth = MathF.Max(maxCardWidth,
                    AntagManifestCardHorizontalPadding + dollWidth + detailRowWidth);
            }

            if (maxCardWidth <= 0f)
                return DefaultWindowSize.X;

            return RoundEndSummaryWindowHorizontalPadding + maxCardWidth;
        }

        private float GetPlayerManifestRequiredWindowWidth(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var maxCardWidth = 0f;

            foreach (var playerInfo in playersInfo)
            {
                var detailRowWidth = MeasureDetailRowWidth(
                    Loc.GetString("round-end-summary-window-player-manifest-ooc",
                        ("playerOOCName", playerInfo.PlayerOOCName)),
                    GetPlayerManifestRoleText(playerInfo));

                var dollWidth = HasDrawableManifestDoll(playerInfo.PlayerNetEntity)
                    ? PlayerManifestDollWidth + PlayerManifestCardSeparation
                    : 0f;

                maxCardWidth = MathF.Max(maxCardWidth,
                    AntagManifestCardHorizontalPadding + dollWidth + detailRowWidth);
            }

            if (maxCardWidth <= 0f)
                return DefaultWindowSize.X;

            return RoundEndSummaryWindowHorizontalPadding + maxCardWidth;
        }

        private static float MeasureDetailRowWidth(string left, string right)
        {
            var leftWidth = MeasureSubTextWidth(left);
            var rightWidth = MeasureSubTextWidth(right);

            return leftWidth + rightWidth + DetailRowSeparation;
        }

        private static float MeasureSubTextWidth(string text)
        {
            var label = new Label
            {
                Text = text,
                StyleClasses = { "LabelSubText" },
            };

            label.Measure(new Vector2(float.PositiveInfinity, float.PositiveInfinity));

            return MathF.Max(label.DesiredSize.X, text.Length * ApproximateSubTextGlyphWidth);
        }

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

            var doll = MakePlayerManifestDoll(playerInfo);
            if (doll != null)
                card.AddChild(doll);

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

        private Control? MakePlayerManifestDoll(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            return MakeManifestDoll(playerInfo.PlayerNetEntity, new Vector2(60, 60), new Vector2(58, 58), new Vector2(1.45f, 1.45f));
        }

        private Control MakeAntagManifestSection(RoundEndMessageEvent.RoundEndPlayerInfo[] playersInfo)
        {
            var antags = playersInfo
                .Where(player => player.ShowInAntagManifest)
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

            var firstAntagCard = true;
            foreach (var playerInfo in antags)
            {
                if (!firstAntagCard)
                    root.AddChild(MakeManifestCardSeparator());

                root.AddChild(MakeAntagManifestCard(playerInfo));
                firstAntagCard = false;
            }

            return root;
        }

        private static Control MakeManifestCardSeparator()
        {
            return new PanelContainer
            {
                MinHeight = 1,
                HorizontalExpand = true,
                Margin = new Thickness(8, 0),
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = ManifestCardSeparatorColor,
                },
            };
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

            var doll = MakeAntagDoll(playerInfo);
            if (doll != null)
                card.AddChild(doll);

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
                GetPlayerManifestRoleText(playerInfo)));

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
                SeparationOverride = DetailRowSeparation,
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
                MinWidth = MeasureSubTextWidth(text),
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
            var root = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 4,
                HorizontalExpand = true,
                Margin = new Thickness(0, 4, 0, 0),
            };

            root.AddChild(new Label
            {
                Text = Loc.GetString("round-end-summary-window-objectives-table-title"),
                StyleClasses = { "LabelHeading" },
                HorizontalExpand = true,
            });

            var table = new TableContainer
            {
                Columns = 2,
                MinForcedColumnWidth = 170,
                HorizontalExpand = true,
            };

            AddObjectivesTableRow(
                table,
                Loc.GetString("round-end-summary-window-objectives-table-objective"),
                Loc.GetString("round-end-summary-window-objectives-table-status"),
                header: true);

            foreach (var objective in objectives)
            {
                var (status, color) = GetObjectiveStatus(objective.Progress);
                AddObjectivesTableRow(table, objective.Title, status, color);
            }

            root.AddChild(table);
            return root;
        }

        private static void AddObjectivesTableRow(
            TableContainer table,
            string objective,
            string status,
            Color? statusColor = null,
            bool header = false)
        {
            var objectiveLabel = new RichTextLabel
            {
                HorizontalExpand = true,
                VerticalAlignment = VAlignment.Top,
            };
            objectiveLabel.SetMarkup(header
                ? $"[bold]{FormattedMessage.EscapeText(objective)}[/bold]"
                : FormattedMessage.EscapeText(objective));

            var statusLabel = new Label
            {
                Text = status,
                StyleClasses = { header ? "LabelSubText" : "Label" },
                ClipText = true,
                MinWidth = 150,
                HorizontalExpand = true,
                HorizontalAlignment = HAlignment.Right,
                VerticalAlignment = VAlignment.Top,
            };

            if (statusColor != null)
                statusLabel.FontColorOverride = statusColor.Value;

            table.AddChild(objectiveLabel);
            table.AddChild(statusLabel);
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

        private Control? MakeAntagDoll(RoundEndMessageEvent.RoundEndPlayerInfo playerInfo)
        {
            return MakeManifestDoll(playerInfo.PlayerNetEntity, new Vector2(160, 160), new Vector2(160, 160), new Vector2(3f, 3f));
        }

        public void ClearManifestDollSnapshots()
        {
            foreach (var view in _manifestDollViews)
            {
                view.ClearSnapshot();
            }

            _manifestDollViews.Clear();
        }

        private Control? MakeManifestDoll(NetEntity? playerNetEntity, Vector2 panelSize, Vector2 viewSize, Vector2 scale)
        {
            if (!HasDrawableManifestDoll(playerNetEntity))
                return null;

            var panel = new PanelContainer
            {
                SetSize = panelSize,
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = ManifestBodyBackground,
                    BorderColor = ManifestPanelBorder,
                    BorderThickness = new Thickness(1),
                },
            };

            var view = new RoundEndManifestDollView(playerNetEntity, _entityManager)
            {
                OverrideDirection = Direction.South,
                SetSize = viewSize,
                Scale = scale,
                HorizontalAlignment = HAlignment.Center,
                VerticalAlignment = VAlignment.Center,
            };
            view.SnapshotFailed += () => panel.Visible = false;
            _manifestDollViews.Add(view);
            panel.AddChild(view);

            return panel;
        }

        private bool HasDrawableManifestDoll(NetEntity? playerNetEntity)
        {
            var spriteSystem = _entityManager.System<SpriteSystem>();

            if (playerNetEntity != null &&
                _entityManager.TryGetEntity(playerNetEntity, out var source) &&
                source != null &&
                !_entityManager.Deleted(source.Value) &&
                _entityManager.TryGetComponent(source.Value, out SpriteComponent? sourceSprite))
            {
                spriteSystem.ForceUpdate(source.Value);
                if (HasDrawableSprite(sourceSprite, GetTypingIndicatorLayer(spriteSystem, source.Value, sourceSprite)))
                    return true;
            }

            return HasDrawableFallbackDoll(spriteSystem);
        }

        private bool HasDrawableFallbackDoll(SpriteSystem spriteSystem)
        {
            if (_fallbackDollDrawable != null)
                return _fallbackDollDrawable.Value;

            EntityUid? fallback = null;
            try
            {
                fallback = _entityManager.Spawn(ManifestFallbackPrototype);
                spriteSystem.ForceUpdate(fallback.Value);

                _fallbackDollDrawable = _entityManager.TryGetComponent(fallback.Value, out SpriteComponent? fallbackSprite) &&
                                        HasDrawableSprite(fallbackSprite);
            }
            finally
            {
                if (fallback != null)
                    _entityManager.DeleteEntity(fallback.Value);
            }

            return _fallbackDollDrawable.Value;
        }

        private static bool HasDrawableSprite(SpriteComponent sprite, int? ignoredLayer = null)
        {
            if (!sprite.Visible)
                return false;

            var index = 0;
            foreach (var layer in sprite.AllLayers)
            {
                if (ignoredLayer == index++)
                    continue;

                if (layer.Visible &&
                    (layer.Texture != null || layer.RsiState.IsValid && layer.ActualRsi != null))
                {
                    return true;
                }
            }

            return false;
        }

        private static int? GetTypingIndicatorLayer(SpriteSystem spriteSystem, EntityUid uid, SpriteComponent sprite)
        {
            return spriteSystem.LayerMapTryGet((uid, sprite), TypingIndicatorLayers.Base, out var layer, false)
                ? layer
                : null;
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

        private sealed class RoundEndManifestDollView : SpriteView
        {
            private const string SnapshotPrototype = "clientsideclone";
            private const float SourceSnapshotRefreshDuration = 2f;

            private readonly NetEntity? _sourceNetEntity;
            private EntityUid? _snapshotEntity;
            private bool _sourceRefreshComplete;
            private float _sourceSnapshotRefreshTime;

            public event Action? SnapshotFailed;

            public RoundEndManifestDollView(NetEntity? sourceNetEntity, IEntityManager entityManager) : base(entityManager)
            {
                _sourceNetEntity = sourceNetEntity;
            }

            protected override void EnteredTree()
            {
                base.EnteredTree();

                if (_snapshotEntity != null)
                    return;

                RefreshSnapshot();
            }

            protected override void FrameUpdate(FrameEventArgs args)
            {
                base.FrameUpdate(args);

                if (_sourceRefreshComplete || _sourceNetEntity == null)
                    return;

                if (!TrySnapshotSource())
                    return;

                _sourceSnapshotRefreshTime += args.DeltaSeconds;
                if (_sourceSnapshotRefreshTime >= SourceSnapshotRefreshDuration)
                    _sourceRefreshComplete = true;
            }

            private void RefreshSnapshot()
            {
                if (TrySnapshotSource())
                    return;

                if (!SnapshotFallback())
                    SnapshotFailed?.Invoke();
            }

            private bool TrySnapshotSource()
            {
                if (_sourceNetEntity == null ||
                    !EntMan.TryGetEntity(_sourceNetEntity, out var source) ||
                    source == null ||
                    EntMan.Deleted(source.Value) ||
                    !EntMan.TryGetComponent(source.Value, out SpriteComponent? sourceSprite))
                {
                    return false;
                }

                SpriteSystem ??= EntMan.System<SpriteSystem>();
                SpriteSystem.ForceUpdate(source.Value);
                if (!HasDrawableSprite(sourceSprite, GetTypingIndicatorLayer(SpriteSystem, source.Value, sourceSprite)))
                    return false;

                return SnapshotSprite(source.Value, sourceSprite);
            }

            private bool SnapshotFallback()
            {
                SpriteSystem ??= EntMan.System<SpriteSystem>();

                var fallback = EntMan.Spawn(ManifestFallbackPrototype);

                try
                {
                    SpriteSystem.ForceUpdate(fallback);
                    return EntMan.TryGetComponent(fallback, out SpriteComponent? fallbackSprite) &&
                           SnapshotSprite(fallback, fallbackSprite);
                }
                finally
                {
                    EntMan.DeleteEntity(fallback);
                }
            }

            private bool SnapshotSprite(EntityUid source, SpriteComponent sourceSprite)
            {
                SpriteSystem ??= EntMan.System<SpriteSystem>();
                SpriteSystem.ForceUpdate(source);

                var snapshot = _snapshotEntity;
                var createdSnapshot = false;
                if (snapshot == null || EntMan.Deleted(snapshot.Value))
                {
                    snapshot = EntMan.Spawn(SnapshotPrototype);
                    _snapshotEntity = snapshot;
                    SetEntity(snapshot);
                    createdSnapshot = true;
                }

                var snapshotUid = snapshot.Value;
                var snapshotSprite = EntMan.GetComponent<SpriteComponent>(snapshotUid);
                SpriteSystem.CopySprite((source, sourceSprite), (snapshotUid, snapshotSprite));
                HideTypingIndicator(snapshotUid, snapshotSprite);

                if (!HasDrawableSprite(snapshotSprite))
                {
                    if (createdSnapshot)
                        ClearSnapshot();

                    return false;
                }

                SpriteSystem.ForceUpdate(snapshotUid);
                return true;
            }

            private void HideTypingIndicator(EntityUid snapshot, SpriteComponent snapshotSprite)
            {
                if (SpriteSystem == null ||
                    !SpriteSystem.LayerMapTryGet((snapshot, snapshotSprite), TypingIndicatorLayers.Base, out var layer, false))
                {
                    return;
                }

                SpriteSystem.LayerSetVisible((snapshot, snapshotSprite), layer, false);
            }

            public void ClearSnapshot()
            {
                SetEntity(null);

                if (_snapshotEntity == null)
                    return;

                EntMan.TryQueueDeleteEntity(_snapshotEntity.Value);
                _snapshotEntity = null;
            }
        }
        // DS14-end
    }

}
