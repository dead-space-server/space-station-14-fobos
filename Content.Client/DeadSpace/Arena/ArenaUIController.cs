// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using System.Linq;
using Content.Client.Gameplay;
using Content.Shared.DeadSpace.Arena;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.DeadSpace.Arena;

public sealed class ArenaUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;

    private PanelContainer? _arenaPanel;
    private Label? _arenaLabel;
    private PanelContainer? _votePanel;
    private Button? _voteDmButton;
    private Button? _voteTdmButton;
    private Label? _voteStateLabel;

    private ArenaMode _mode;
    private ArenaRoundState _roundState;
    private float _timeRemaining = -1f;
    private float _localTimer;
    private int _blueKills;
    private int _redKills;
    private List<ArenaMode> _availableModes = new();
    private Dictionary<NetEntity, ArenaMode> _votes = new();
    private int _lastDmVotes;
    private int _lastTdmVotes;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaRoundUpdateEvent>(OnRoundUpdate);
        SubscribeNetworkEvent<ArenaVoteStateEvent>(OnVoteState);
    }

    public void OnStateEntered(GameplayState state)
    {
        CreateArenaPanel();
        CreateVotePanel();
    }

    public void OnStateExited(GameplayState state)
    {
        RemoveArenaPanel();
        RemoveVotePanel();
    }

    private void CreateArenaPanel()
    {
        if (_arenaPanel != null)
            return;

        _arenaLabel = new Label
        {
            Text = "",
            FontColorOverride = Color.White,
        };
        _arenaPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = new Color(0, 0, 0, 0.6f),
                ContentMarginLeftOverride = 10,
                ContentMarginRightOverride = 10,
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 4,
            },
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Top,
            Margin = new Thickness(0, 8, 0, 0),
            Visible = false,
            Children = { _arenaLabel }
        };
        UIManager.RootControl.AddChild(_arenaPanel);
    }

    private void RemoveArenaPanel()
    {
        if (_arenaPanel == null)
            return;
        UIManager.RootControl.RemoveChild(_arenaPanel);
        _arenaPanel.Dispose();
        _arenaPanel = null;
        _arenaLabel = null;
    }

    private void CreateVotePanel()
    {
        if (_votePanel != null)
            return;

        _voteDmButton = new Button
        {
            Text = Loc.GetString("arena-mode-deathmatch"),
            MinHeight = 30,
        };
        _voteDmButton.OnPressed += _ => SendVote(ArenaMode.Deathmatch);
        _voteTdmButton = new Button
        {
            Text = Loc.GetString("arena-mode-tdm"),
        };
        _voteTdmButton.OnPressed += _ => SendVote(ArenaMode.TDM);

        _voteStateLabel = new Label
        {
            Text = "",
            FontColorOverride = Color.White,
            HorizontalAlignment = Control.HAlignment.Center,
        };
        _votePanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = new Color(0, 0, 0, 0.6f),
                ContentMarginLeftOverride = 10,
                ContentMarginRightOverride = 10,
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 4,
            },
            HorizontalAlignment = Control.HAlignment.Center,
            VerticalAlignment = Control.VAlignment.Top,
            Margin = new Thickness(0, 50, 0, 0),
            Visible = false,
            Children =
            {
                new BoxContainer
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical,
                    SeparationOverride = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = "Голосование за режим",
                            FontColorOverride = Color.White,
                            HorizontalAlignment = Control.HAlignment.Center,
                        },
                        new BoxContainer
                        {
                            Orientation = BoxContainer.LayoutOrientation.Horizontal,
                            SeparationOverride = 8,
                            HorizontalAlignment = Control.HAlignment.Center,
                            Children = { _voteDmButton, _voteTdmButton }
                        },
                        _voteStateLabel
                    }
                }
            }
        };
        UIManager.RootControl.AddChild(_votePanel);
    }

    private void RemoveVotePanel()
    {
        if (_votePanel == null)
            return;
        UIManager.RootControl.RemoveChild(_votePanel);
        _votePanel.Dispose();
        _votePanel = null;
        _voteDmButton = null;
        _voteTdmButton = null;
        _voteStateLabel = null;
    }

    private void SendVote(ArenaMode vote)
    {
        _net.SendSystemNetworkMessage(new ArenaVoteCastEvent(vote));
    }

    private void OnRoundUpdate(ArenaRoundUpdateEvent ev, EntitySessionEventArgs args)
    {
        _mode = ev.Mode;
        _roundState = ev.RoundState;
        _timeRemaining = ev.TimeRemaining;
        _localTimer = 0f;
        _blueKills = ev.BlueKills;
        _redKills = ev.RedKills;
    }

    private void OnVoteState(ArenaVoteStateEvent ev, EntitySessionEventArgs args)
    {
        _availableModes = ev.AvailableModes;
        _votes = ev.Votes;
        _lastDmVotes = ev.Votes.Values.Count(v => v == ArenaMode.Deathmatch);
        _lastTdmVotes = ev.Votes.Values.Count(v => v == ArenaMode.TDM);
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        UpdateArenaPanel(args);
        UpdateVotePanel();
    }

    private bool IsPlayerOnArena()
    {
        var ent = _playerMan.LocalEntity;
        return ent.HasValue && EntityManager.HasComponent<ArenaPlayerComponent>(ent.Value);
    }

    private void UpdateArenaPanel(FrameEventArgs args)
    {
        if (_arenaPanel == null || _arenaLabel == null)
            return;
        if (!IsPlayerOnArena() || _timeRemaining < 0f)
        {
            _arenaPanel.Visible = false;
            return;
        }
        _arenaPanel.Visible = true;
        _localTimer += (float)args.DeltaSeconds;
        var remaining = Math.Max(0f, _timeRemaining - _localTimer);
        if (remaining <= 0f)
        {
            _arenaPanel.Visible = false;
            return;
        }
        _arenaLabel.Text = FormatArenaText(remaining);
    }

    private string FormatArenaText(float remaining)
    {
        var modeName = _mode switch
        {
            ArenaMode.Deathmatch => Loc.GetString("arena-mode-deathmatch"),
            ArenaMode.TDM => Loc.GetString("arena-mode-tdm"),
            _ => "Unknown"
        };
        var statePrefix = _roundState switch
        {
            ArenaRoundState.Intermission => $"[{Loc.GetString("arena-state-intermission")}] ",
            ArenaRoundState.Preparation => $"[{Loc.GetString("arena-state-preparation")}] ",
            _ => ""
        };
        var score = _mode == ArenaMode.TDM
            ? $" | Синие: {_blueKills} | Красные: {_redKills}"
            : "";
        var minutes = (int)remaining / 60;
        var seconds = (int)remaining % 60;
        return $"{statePrefix}{modeName}{score} | {minutes:D2}:{seconds:D2}";
    }

    private void UpdateVotePanel()
    {
        if (_votePanel == null || _voteDmButton == null || _voteTdmButton == null || _voteStateLabel == null)
            return;
        if (!IsPlayerOnArena() || _timeRemaining < 0f || _roundState != ArenaRoundState.Intermission)
        {
            _votePanel.Visible = false;
            return;
        }
        _votePanel.Visible = true;
        _voteDmButton.Visible = _availableModes.Contains(ArenaMode.Deathmatch);
        _voteTdmButton.Visible = _availableModes.Contains(ArenaMode.TDM);
        var totalVotes = _lastDmVotes + _lastTdmVotes;
        _voteStateLabel.Text = totalVotes > 0
            ? $"Deathmatch: {_lastDmVotes} | TDM: {_lastTdmVotes}"
            : "Нажмите на кнопку, чтобы проголосовать";
    }
}
