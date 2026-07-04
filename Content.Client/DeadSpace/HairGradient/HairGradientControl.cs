// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.DeadSpace.HairGradient;

public sealed class HairGradientControl : BoxContainer
{
    public event Action<bool, Color>? OnGradientChanged;

    private readonly CheckBox _gradientCheckbox;
    private readonly BoxContainer _gradientColorContainer;
    private readonly ColorSelectorSliders _gradientColorPicker;
    private bool _updating;

    public bool GradientEnabled
    {
        get => _gradientCheckbox.Pressed;
        set
        {
            if (_gradientCheckbox.Pressed != value)
                _gradientCheckbox.Pressed = value;
        }
    }

    public Color GradientColor
    {
        get => _gradientColorPicker.Color;
        set => _gradientColorPicker.Color = value;
    }

    public HairGradientControl()
    {
        Orientation = LayoutOrientation.Vertical;
        HorizontalExpand = true;

        _gradientCheckbox = new CheckBox
        {
            Text = Loc.GetString("hair-gradient-toggle"),
            HorizontalExpand = true,
        };

        _gradientColorContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            HorizontalExpand = true,
            Visible = false,
        };

        _gradientColorPicker = new ColorSelectorSliders
        {
            HorizontalExpand = true,
        };
        _gradientColorPicker.SelectorType = ColorSelectorSliders.ColorSelectorType.Hsv;

        _gradientColorContainer.AddChild(_gradientColorPicker);
        AddChild(_gradientCheckbox);
        AddChild(_gradientColorContainer);

        _gradientCheckbox.OnToggled += OnCheckboxToggled;
        _gradientColorPicker.OnColorChanged += OnColorChanged;
    }

    public void SetData(bool enabled, Color color)
    {
        _updating = true;
        if (_gradientCheckbox.Pressed != enabled)
            _gradientCheckbox.Pressed = enabled;
        _gradientColorPicker.Color = color;
        _gradientColorContainer.Visible = enabled;
        _updating = false;
    }

    private void OnCheckboxToggled(BaseButton.ButtonToggledEventArgs args)
    {
        if (_updating) return;
        _gradientColorContainer.Visible = args.Pressed;
        OnGradientChanged?.Invoke(args.Pressed, _gradientColorPicker.Color);
    }

    private void OnColorChanged(Color newColor)
    {
        if (_updating) return;
        if (_gradientCheckbox.Pressed)
            OnGradientChanged?.Invoke(true, newColor);
    }
}
