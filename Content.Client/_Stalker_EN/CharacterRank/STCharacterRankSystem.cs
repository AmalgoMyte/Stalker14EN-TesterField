using Content.Shared._ES.Viewcone;
using Content.Shared._Stalker_EN.CharacterRank;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Stalker_EN.CharacterRank;

/// <summary>
/// Displays the character's rank icon as a status icon below the band icon.
/// </summary>
public sealed class STCharacterRankSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STCharacterRankComponent, GetStatusIconsEvent>(OnGetStatusIcon);
    }

    private void OnGetStatusIcon(EntityUid uid, STCharacterRankComponent comp, ref GetStatusIconsEvent args)
    {
        if (TryComp<ESViewconeOccludableComponent>(uid, out var occ) && occ.IsHidden)
            return;

        if (_proto.TryIndex(comp.RankIconId, out var icon))
            args.StatusIcons.Add(icon);
    }
}
