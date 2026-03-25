using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Overlays;

[RegisterComponent, NetworkedComponent]
public sealed partial class NVHudComponent : SwitchableVisionOverlayComponent
{
    public override EntProtoId? ToggleAction { get; set; } = "ToggleNVHud";

    public override Color Color { get; set; } = Color.FromHex("#d4d4d4"); // Mono
}

public sealed partial class ToggleNVHudEvent : InstantActionEvent;
