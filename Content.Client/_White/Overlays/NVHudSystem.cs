using Content.Client.Overlays;
using Content.Shared._White.Overlays;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Client._White.Overlays;

public sealed class NVHudSystem : EquipmentHudSystem<NVHudComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private BaseSwitchableOverlay<NVHudComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NVHudComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _overlay = new BaseSwitchableOverlay<NVHudComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<NVHudComponent> ent,
        ref RefreshEquipmentHudEvent<NVHudComponent> args)
    {
        if (!ent.Comp.IsEquipment)
            base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<NVHudComponent> ent,
        ref InventoryRelayedEvent<RefreshEquipmentHudEvent<NVHudComponent>> args)
    {
        if (ent.Comp.IsEquipment)
            base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<NVHudComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NVHudComponent> args)
    {
        base.UpdateInternal(args);

        var active = false;
        NVHudComponent? nvComp = null;
        foreach (var comp in args.Components)
        {
            if (comp.IsActive || comp.PulseTime > 0f && comp.PulseAccumulator < comp.PulseTime)
                active = true;
            else
                continue;

            if (comp.DrawOverlay)
            {
                if (nvComp == null)
                    nvComp = comp;
                else if (nvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                    nvComp = comp;
            }

            if (active && nvComp is { PulseTime: <= 0 })
                break;
        }

        UpdateNVHud(active);
        UpdateOverlay(nvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        UpdateNVHud(false);
        UpdateOverlay(null);
    }

    private void UpdateNVHud(bool active)
    {
        Log.Info($"NVHudSystem: Setting DrawLighting to {!active}");

        _lightManager.DrawLighting = !active;
    }

    private void UpdateOverlay(NVHudComponent? nvComp)
    {
        _overlay.Comp = nvComp;

        switch (nvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NVHudComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        if (_overlayMan.TryGetOverlay<BaseSwitchableOverlay<IRHudComponent>>(out var overlay))
            overlay.IsActive = nvComp == null;
    }
}
