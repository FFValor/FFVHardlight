using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mining.Components;

/// <summary>
/// This is a component that, when held in the inventory or pocket of a player, gives the the MiningOverlay.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(MiningScannerSystem))]
public sealed partial class MiningScannerComponent : Component
{
    [DataField]
    public bool Activated = false;

    [DataField]
    public float Range = 5;

    /// <summary>
    /// Whether or not the scanner can be toggled via standard interactions
    /// (alt verbs, using in hand, etc)
    /// </summary>
    [DataField]
    public bool CanInteractUse = true;


///Action
    [DataField]
    public EntProtoId ToggleAction = "ActionToggleScanner";

    [DataField]
    public EntityUid? ToggleActionEntity;
}
