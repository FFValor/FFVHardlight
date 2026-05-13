using Content.Shared._NF.Mining.Components; // Frontier
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mining.Components;
using Content.Shared.Actions;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Content.Shared.Examine;

namespace Content.Shared.Mining;

public sealed partial class MiningScannerSystem : EntitySystem // Frontier: partial
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MiningScannerComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MiningScannerComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<MiningScannerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<MiningScannerComponent, ToggleScannerActionEvent>(OnToggleScanner);
        SubscribeLocalEvent<MiningScannerComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<MiningScannerComponent, ExaminedEvent>(OnExamine);

        NFInitialize(); // Frontier
    }

    private void OnInserted(Entity<MiningScannerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        UpdateViewerComponent(args.Container.Owner);
    }

    private void OnRemoved(Entity<MiningScannerComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        UpdateViewerComponent(args.Container.Owner);
    }

    private void OnToggled(Entity<MiningScannerComponent> ent, ref ItemToggledEvent args)
    {
        ent.Comp.Activated = args.Activated;
        // Sync with ItemToggle if present
        if (TryComp<ItemToggleComponent>(ent.Owner, out var toggle))
        {
            toggle.Activated = ent.Comp.Activated;
            Dirty(ent.Owner, toggle);
        }
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container))
            UpdateViewerComponent(container.Owner);
    }

    private void OnToggleScanner(Entity<MiningScannerComponent> ent, ref ToggleScannerActionEvent args)
    {
        if (_container.TryGetContainingContainer((ent.Owner, null, null), out var container))
        {
            ent.Comp.Activated = !ent.Comp.Activated;
            // Sync with ItemToggle if present
            if (TryComp<ItemToggleComponent>(ent.Owner, out var toggle))
            {
                toggle.Activated = ent.Comp.Activated;
                Dirty(ent.Owner, toggle);
            }
            UpdateViewerComponent(container.Owner, true);
        }
    }


    private void OnGetActions(Entity<MiningScannerComponent> scanner, ref GetItemActionsEvent args)
    {
        if (scanner.Comp.CanInteractUse)
            args.AddAction(ref scanner.Comp.ToggleActionEntity, scanner.Comp.ToggleAction);
    }
    public void UpdateViewerComponent(EntityUid uid, bool useScannerEnabled = false)
    {
        Entity<MiningScannerComponent>? scannerEnt = null;

        var ents = _inventory.GetHandOrInventoryEntities(uid);
        foreach (var ent in ents)
        {
            TryComp<ItemToggleComponent>(ent, out var toggle);
            if (!TryComp<MiningScannerComponent>(ent, out var scannerComponent) ||
                toggle == null && !useScannerEnabled)
                continue;

            if (toggle != null && !toggle.Activated || useScannerEnabled && !scannerComponent.Activated)
                continue;

            if (scannerEnt == null || scannerComponent.Range > scannerEnt.Value.Comp.Range)
                scannerEnt = (ent, scannerComponent);
        }

        if (_net.IsServer)
        {
            if (scannerEnt == null)
            {
                if (TryComp<MiningScannerViewerComponent>(uid, out var viewer))
                    viewer.QueueRemoval = true;
            }
            else
            {
                var viewer = EnsureComp<MiningScannerViewerComponent>(uid);
                viewer.ViewRange = scannerEnt.Value.Comp.Range;
                viewer.QueueRemoval = false;
                viewer.NextPingTime = _timing.CurTime + viewer.PingDelay;
                Dirty(uid, viewer);
            }
        }
    }

    private void OnExamine(Entity<MiningScannerComponent> scanner, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(
            scanner.Comp.Activated
            ? "mineral-scanner-on-examine-is-on-message"
            : "mineral-scanner-on-examine-is-off-message"
            ));
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MiningScannerViewerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var viewer, out var xform))
        {
            if (viewer.QueueRemoval)
            {
                // Frontier: innate mining scanner
                if (TryComp<InnateMiningScannerViewerComponent>(uid, out var innateViewer))
                {
                    SetupInnateMiningViewerComponent((uid, innateViewer));
                }
                else
                {
                    // End Frontier: innate mining scanner
                    RemCompDeferred(uid, viewer);
                    continue;
                } // Frontier
            }

            if (_timing.CurTime < viewer.NextPingTime)
                continue;

            viewer.NextPingTime = _timing.CurTime + viewer.PingDelay;
            viewer.LastPingLocation = xform.Coordinates;
            if (_net.IsClient && _timing.IsFirstTimePredicted)
                _audio.PlayEntity(viewer.PingSound, uid, uid);
        }
    }
}
