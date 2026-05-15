using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Inventory.Events;
using Robust.Shared.GameStates;

namespace Content.Shared.Item.ItemToggle;

/// <summary>
/// Handles <see cref="ComponentTogglerComponent"/> component manipulation.
/// </summary>
public sealed class ComponentTogglerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentTogglerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<ComponentTogglerComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ComponentTogglerComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ComponentTogglerComponent, TogglerComponentActionEvent>(OnTogglerAction);
    }

    private void OnToggled(Entity<ComponentTogglerComponent> ent, ref ItemToggledEvent args)
    {
        ent.Comp.Activated = args.Activated;
        var target = ent.Comp.Parent ? Transform(ent).ParentUid : ent.Owner;

        if (args.Activated)
        {
            EntityManager.AddComponents(target, ent.Comp.Components);
            RaiseLocalEvent(target, new ComponentStartup());
        }
        else
        {
            EntityManager.RemoveComponents(target, ent.Comp.RemoveComponents ?? ent.Comp.Components);
        }

        Dirty(ent, ent.Comp);
    }

    private void OnEquipped(Entity<ComponentTogglerComponent> ent, ref GotEquippedEvent args)
    {
        var target = ent.Comp.Parent ? Transform(ent).ParentUid : ent.Owner;

        EntityManager.AddComponents(target, ent.Comp.Components);
        RaiseLocalEvent(target, new ComponentStartup());
        Dirty(ent, ent.Comp);
    }

    private void OnUnequipped(Entity<ComponentTogglerComponent> ent, ref GotUnequippedEvent args)
    {
        var target = ent.Comp.Parent ? Transform(ent).ParentUid : ent.Owner;

        EntityManager.RemoveComponents(target, ent.Comp.RemoveComponents ?? ent.Comp.Components);
        Dirty(ent, ent.Comp);
    }
    private void OnTogglerAction(Entity<ComponentTogglerComponent> ent, ref TogglerComponentActionEvent args)
    {
        ent.Comp.Activated = !ent.Comp.Activated;
        var target = ent.Comp.Parent ? Transform(ent).ParentUid : ent.Owner;
        // Sync with ItemToggle if present
        if (TryComp<ItemToggleComponent>(ent.Owner, out var toggle))
        {
            toggle.Activated = ent.Comp.Activated;
            Dirty(ent.Owner, toggle);
        }

        if (ent.Comp.Activated)
        {
            EntityManager.AddComponents(target, ent.Comp.Components);
            RaiseLocalEvent(target, new ComponentStartup());
        }
        else
        {
            EntityManager.RemoveComponents(target, ent.Comp.RemoveComponents ?? ent.Comp.Components);
        }

        Dirty(ent, ent.Comp);
    }
}
