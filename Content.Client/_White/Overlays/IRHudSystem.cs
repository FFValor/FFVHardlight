using Content.Client.Overlays;
using Content.Shared._White.Overlays;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Client._White.Overlays;

public sealed class IRHudSystem : EquipmentHudSystem<IRHudComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private IRHudOverlay _IRHudOverlay = default!;
    private BaseSwitchableOverlay<IRHudComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IRHudComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _IRHudOverlay = new IRHudOverlay();
        _overlay = new BaseSwitchableOverlay<IRHudComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<IRHudComponent> ent,
        ref RefreshEquipmentHudEvent<IRHudComponent> args)
    {
        if (!ent.Comp.IsEquipment)
            base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<IRHudComponent> ent,
        ref InventoryRelayedEvent<RefreshEquipmentHudEvent<IRHudComponent>> args)
    {
        if (ent.Comp.IsEquipment)
            base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<IRHudComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<IRHudComponent> args)
    {
        base.UpdateInternal(args);
        IRHudComponent? tvComp = null;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (tvComp == null)
                tvComp = comp;
            else if (!tvComp.DrawOverlay && comp.DrawOverlay)
                tvComp = comp;
            else if (tvComp.DrawOverlay == comp.DrawOverlay && tvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                tvComp = comp;

            lightRadius = MathF.Max(lightRadius, comp.LightRadius);
        }

        UpdateIRHudOverlay(tvComp, lightRadius);
        UpdateOverlay(tvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _IRHudOverlay.ResetLight(false);
        UpdateOverlay(null);
        UpdateIRHudOverlay(null, 0f);
    }

    private void UpdateIRHudOverlay(IRHudComponent? comp, float lightRadius)
    {
        _IRHudOverlay.LightRadius = lightRadius;
        _IRHudOverlay.Comp = comp;

        switch (comp)
        {
            case not null when !_overlayMan.HasOverlay<IRHudOverlay>():
                _overlayMan.AddOverlay(_IRHudOverlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_IRHudOverlay);
                _IRHudOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(IRHudComponent? tvComp)
    {
        _overlay.Comp = tvComp;

        switch (tvComp)
        {
            case { DrawOverlay: true } when !_overlayMan.HasOverlay<BaseSwitchableOverlay<IRHudComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null or { DrawOverlay: false }:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        // Night vision overlay is prioritized
        _overlay.IsActive = !_overlayMan.HasOverlay<BaseSwitchableOverlay<NVHudComponent>>();
    }
}
