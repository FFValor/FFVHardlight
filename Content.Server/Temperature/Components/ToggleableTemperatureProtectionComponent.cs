using Content.Server.Temperature.Systems;

namespace Content.Server.Temperature.Components;

[RegisterComponent]
[Access(typeof(TemperatureSystem))]
public sealed partial class ToggleableTemperatureProtectionComponent : Component
{
    /// <summary>
    ///     Multiplier for the transferred heat when heating up while active
    /// </summary>
    [DataField]
    public float ActivatedHeatingCoefficient = 1.0f;

    /// <summary>
    ///     Multiplier for the transferred heat when cooling down while active
    /// </summary>
    [DataField]
    public float ActivatedCoolingCoefficient = 1.0f;

    /// <summary>
    ///     Multiplier for the transferred heat when heating up while inactive
    /// </summary>
    [DataField]
    public float DeactivatedHeatingCoefficient = 1.0f;

    /// <summary>
    ///     Multiplier for the transferred heat when cooling down while inactive
    /// </summary>
    [DataField]
    public float DeactivatedCoolingCoefficient = 1.0f;
}
