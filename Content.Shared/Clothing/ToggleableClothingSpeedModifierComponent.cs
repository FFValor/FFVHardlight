using Robust.Shared.GameStates;

namespace Content.Shared.Clothing;

/// <summary>
/// Allows toggling the values of a <see cref="ClothingSpeedModifierComponent"/> between activated and deactivated states.
/// Requires <see cref="ItemToggleComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleableClothingSpeedModifierComponent : Component
{
    /// <summary>
    /// Walk modifier when activated.
    /// </summary>
    [DataField]
    public float ActivatedWalkModifier = 1.0f;

    /// <summary>
    /// Sprint modifier when activated.
    /// </summary>
    [DataField]
    public float ActivatedSprintModifier = 1.0f;

    /// <summary>
    /// Walk modifier when deactivated.
    /// </summary>
    [DataField]
    public float DeactivatedWalkModifier = 1.0f;

    /// <summary>
    /// Sprint modifier when deactivated.
    /// </summary>
    [DataField]
    public float DeactivatedSprintModifier = 1.0f;
}
