using Content.Shared.Containers.ItemSlots; // HardLight
using Robust.Shared.GameStates;

namespace Content.Shared._Mono.ArmorPlate;

/// <summary>
/// Component for clothes that can hold an armor plate in a dedicated slot. // HardLight
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ArmorPlateHolderComponent : Component
{
    // HardLight start: Moved plate storage to a dedicated item slot to simplify logic and allow for better interactions with container systems.
    public const string PlateSlotId = "armor_plate";

    /// <summary>
    /// The item slot used to hold the installed armor plate.
    /// </summary>
    [DataField("plateSlot")]
    public ItemSlot PlateSlot = new();
    // HardLight end

    /// <summary>
    /// Reference to the currently active armor plate entity.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public EntityUid? ActivePlate;

    /// <summary>
    /// Whether to show a popup notification when the active plate is destroyed.
    /// </summary>
    [DataField]
    public bool ShowBreakPopup = true;

    /// <summary>
    /// Walk speed modifier from the currently active plate.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public float WalkSpeedModifier = 1.0f;

    /// <summary>
    /// Sprint speed modifier from the currently active plate.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public float SprintSpeedModifier = 1.0f;

    /// <summary>
    /// Stamina damage multiplier from the currently active plate.
    /// </summary>
    [DataField]
    public float StaminaDamageMultiplier = 1.0f;

}

