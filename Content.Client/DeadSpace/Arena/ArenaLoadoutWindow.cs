using System.Numerics;
using Content.Shared.DeadSpace.Arena;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Input;

namespace Content.Client.DeadSpace.Arena;

public sealed class ArenaLoadoutWindow : DefaultWindow
{
    public event Action<int>? OnLoadoutConfirmed;

    private int _weaponSelection = -1;
    private ArenaWeaponCard? _selectedCard;
    private readonly BoxContainer _categoriesContainer;
    private readonly Button _confirmButton;

    public ArenaLoadoutWindow()
    {
        Title = Loc.GetString("arena-loadout-title");
        MinSize = new Vector2(450, 500);
        SetSize = new Vector2(450, 500);

        var outerContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            VerticalExpand = true,
        };

        var subtitle = new Label
        {
            Text = Loc.GetString("arena-loadout-subtitle"),
            HorizontalAlignment = HAlignment.Center,
            Margin = new Thickness(0, 0, 0, 6),
        };
        outerContainer.AddChild(subtitle);

        _categoriesContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            HorizontalExpand = true,
            SeparationOverride = 4,
        };

        var scroll = new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Margin = new Thickness(6, 0),
        };
        scroll.AddChild(_categoriesContainer);
        outerContainer.AddChild(scroll);

        _confirmButton = new Button
        {
            Text = Loc.GetString("arena-loadout-confirm"),
            Disabled = true,
            Margin = new Thickness(8, 6),
        };
        _confirmButton.OnPressed += _ =>
        {
            if (_weaponSelection >= 0)
                OnLoadoutConfirmed?.Invoke(_weaponSelection);
        };
        outerContainer.AddChild(_confirmButton);

        Contents.AddChild(outerContainer);
    }

    public void UpdateState(ArenaLoadoutEuiState state)
    {
        _categoriesContainer.RemoveAllChildren();
        _selectedCard = null;
        _weaponSelection = -1;
        _confirmButton.Disabled = true;

        var categories = new List<(string Category, List<ArenaLoadoutOption> Options)>();
        var categoryMap = new Dictionary<string, List<ArenaLoadoutOption>>();

        foreach (var option in state.Weapons)
        {
            var category = Loc.GetString(option.Category);
            if (!categoryMap.TryGetValue(category, out var list))
            {
                list = new List<ArenaLoadoutOption>();
                categoryMap[category] = list;
                categories.Add((category, list));
            }
            list.Add(option);
        }

        foreach (var (category, options) in categories)
        {
            var header = new Label
            {
                Text = category,
                Margin = new Thickness(4, 6, 0, 2),
            };
            _categoriesContainer.AddChild(header);

            foreach (var option in options)
            {
                var card = new ArenaWeaponCard(
                    option.Index,
                    Loc.GetString(option.Name),
                    option.SpritePrototype,
                    Loc.GetString(option.Description));
                card.OnSelected += OnCardSelected;
                _categoriesContainer.AddChild(card);
            }
        }
    }

    private void OnCardSelected(ArenaWeaponCard card)
    {
        _selectedCard?.SetSelected(false);
        _selectedCard = card;
        _weaponSelection = card.WeaponIndex;
        card.SetSelected(true);
        _confirmButton.Disabled = false;
    }

    private sealed class ArenaWeaponCard : PanelContainer
    {
        public event Action<ArenaWeaponCard>? OnSelected;
        public int WeaponIndex { get; }

        private bool _isSelected;
        private static readonly StyleBoxFlat _selectedStyle = new()
        {
            BackgroundColor = new Color(0.2f, 0.45f, 0.2f),
            BorderColor = new Color(0.3f, 0.9f, 0.3f),
            BorderThickness = new Thickness(2, 2, 2, 2),
        };
        private static readonly StyleBoxFlat _defaultStyle = new()
        {
            BackgroundColor = new Color(0.1f, 0.1f, 0.12f),
            BorderColor = new Color(0.2f, 0.2f, 0.25f),
            BorderThickness = new Thickness(1, 1, 1, 1),
        };

        public ArenaWeaponCard(int weaponIndex, string weaponName, string? spritePrototype, string? tooltip = null)
        {
            WeaponIndex = weaponIndex;
            MouseFilter = MouseFilterMode.Stop;
            MinHeight = 56;
            HorizontalExpand = true;

            PanelOverride = _defaultStyle;

            if (!string.IsNullOrEmpty(tooltip))
            {
                ToolTip = tooltip;
            }

            var hbox = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                HorizontalExpand = true,
                VerticalExpand = true,
            };

            if (!string.IsNullOrEmpty(spritePrototype))
            {
                var spriteView = new EntityPrototypeView
                {
                    MinSize = new Vector2(48, 48),
                    SetSize = new Vector2(48, 48),
                    HorizontalAlignment = HAlignment.Center,
                    VerticalAlignment = VAlignment.Center,
                    OverrideDirection = Direction.South,
                };
                spriteView.SetPrototype(spritePrototype);
                hbox.AddChild(spriteView);
            }

            var nameLabel = new Label
            {
                Text = weaponName,
                VerticalAlignment = VAlignment.Center,
                HorizontalExpand = true,
                Margin = new Thickness(8, 0, 0, 0),
            };
            hbox.AddChild(nameLabel);
            AddChild(hbox);

            OnKeyBindDown += args =>
            {
                if (args.Function != EngineKeyFunctions.UIClick)
                    return;
                OnSelected?.Invoke(this);
                args.Handle();
            };
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            PanelOverride = selected ? _selectedStyle : _defaultStyle;
        }
    }
}
