using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Wires;
using Content.Shared.Atmos;
using Content.Shared.Wires;

namespace Content.Server.Atmos;

public sealed partial class PressureWireAction : ComponentWireAction<PressureProtectionComponent>
{
    public override Color Color { get; set; } = Color.Cyan;
    public override string Name { get; set; } = "wire-name-pressure";
    public override bool LightRequiresPower { get; set; } = false;
    public override object StatusKey { get; } = PressureProtectionWireStatus.Status;

    [DataField("pulseTimeout")]
    private float _pulseTimeout = 5f;

    private BarotraumaSystem _barotrauma = default!;

    public override void Initialize()
    {
        base.Initialize();
        _barotrauma = EntityManager.System<BarotraumaSystem>();
    }

    public override StatusLightState? GetLightState(Wire wire, PressureProtectionComponent component)
    {
        return _barotrauma.IsPressureProtectionEnabled(wire.Owner)
            ? StatusLightState.On
            : StatusLightState.Off;
    }

    public override bool Cut(EntityUid user, Wire wire, PressureProtectionComponent component)
    {
        WiresSystem.TryCancelWireAction(wire.Owner, PulseTimeoutKey.Key);
        _barotrauma.SetPressureProtectionEnabled(wire.Owner, false);
        return true;
    }

    public override bool Mend(EntityUid user, Wire wire, PressureProtectionComponent component)
    {
        _barotrauma.SetPressureProtectionEnabled(wire.Owner, true);
        return true;
    }

    public override void Pulse(EntityUid user, Wire wire, PressureProtectionComponent component)
    {
        _barotrauma.SetPressureProtectionEnabled(wire.Owner, false);
        WiresSystem.StartWireAction(wire.Owner, _pulseTimeout, PulseTimeoutKey.Key,
            new TimedWireEvent(AwaitPulseCancel, wire));
    }

    private void AwaitPulseCancel(Wire wire)
    {
        if (!wire.IsCut)
            _barotrauma.SetPressureProtectionEnabled(wire.Owner, true);
    }

    private enum PulseTimeoutKey : byte
    {
        Key,
    }
}