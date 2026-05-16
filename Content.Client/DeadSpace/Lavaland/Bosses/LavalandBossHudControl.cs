using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DeadSpace.Lavaland.Bosses;

public sealed class LavalandBossHudControl : PanelContainer
{
    private readonly Label _bossName;
    private readonly Label _participants;
    private readonly Label _healthText;
    private readonly ProgressBar _healthBar;

    public LavalandBossHudControl()
    {
        MouseFilter = MouseFilterMode.Ignore;
        HorizontalAlignment = HAlignment.Center;
        MinSize = new Vector2(280, 48);
        PanelOverride = new StyleBoxFlat
        {
            BackgroundColor = Color.FromHex("#25252aee"),
            BorderColor = Color.FromHex("#4a4a55"),
            BorderThickness = new Thickness(1),
            ContentMarginLeftOverride = 10,
            ContentMarginRightOverride = 10,
            ContentMarginTopOverride = 6,
            ContentMarginBottomOverride = 6,
        };

        var root = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            SeparationOverride = 4,
            HorizontalExpand = true,
        };
        AddChild(root);

        var titleRow = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };
        root.AddChild(titleRow);

        _bossName = new Label
        {
            Text = "Boss",
            ClipText = true,
            HorizontalExpand = true,
            FontColorOverride = Color.FromHex("#f0f0f0"),
        };
        titleRow.AddChild(_bossName);

        _participants = new Label
        {
            Text = Loc.GetString("lavaland-boss-hud-participants", ("count", 0)),
            Align = Label.AlignMode.Right,
            FontColorOverride = Color.FromHex("#b8b8c0"),
        };
        titleRow.AddChild(_participants);

        _healthBar = new ProgressBar
        {
            HorizontalExpand = true,
            MinValue = 0,
            MaxValue = 1,
            Value = 1,
            SetHeight = 10,
            BackgroundStyleBoxOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#121218") },
            ForegroundStyleBoxOverride = new StyleBoxFlat { BackgroundColor = Color.FromHex("#9f3a3a") },
        };
        root.AddChild(_healthBar);

        _healthText = new Label
        {
            Align = Label.AlignMode.Center,
            Text = "0 / 0",
            FontColorOverride = Color.FromHex("#d8d8df"),
        };
        root.AddChild(_healthText);
    }

    public void UpdateState(string bossName, float currentHealth, float maxHealth, int participants)
    {
        maxHealth = MathF.Max(1f, maxHealth);
        currentHealth = Math.Clamp(currentHealth, 0f, maxHealth);

        _bossName.Text = bossName;
        _participants.Text = Loc.GetString("lavaland-boss-hud-participants", ("count", participants));
        _healthBar.Value = currentHealth / maxHealth;
        _healthText.Text = $"{MathF.Ceiling(currentHealth)} / {MathF.Ceiling(maxHealth)}";
    }
}
