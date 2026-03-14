using Content.Shared._Stalker_EN.Camera;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Stalker_EN.Camera;

/// <summary>
/// Serves photo image data on demand to clients and manages photo BUI state.
/// </summary>
public sealed class STPhotoSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    /// <summary>
    /// Rate limiting: last request time per player.
    /// </summary>
    private readonly Dictionary<NetUserId, TimeSpan> _lastPhotoRequest = new();

    private static readonly TimeSpan RequestCooldown = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<STPhotoComponent, BeforeActivatableUIOpenEvent>(OnBeforeUI);
        SubscribeNetworkEvent<STPhotoRequestEvent>(OnPhotoRequest);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
    }

    private void OnBeforeUI(EntityUid uid, STPhotoComponent comp, BeforeActivatableUIOpenEvent args)
    {
        var state = new STPhotoBoundUiState(comp.PhotoId, comp.PhotographerName, comp.TimeTaken);
        _ui.SetUiState(uid, STPhotoUiKey.Key, state);
    }

    private void OnPhotoRequest(STPhotoRequestEvent ev, EntitySessionEventArgs args)
    {
        var userId = args.SenderSession.UserId;
        var now = _timing.CurTime;

        // Rate limit: 1 request per second per session
        if (_lastPhotoRequest.TryGetValue(userId, out var lastRequest) && now - lastRequest < RequestCooldown)
            return;

        _lastPhotoRequest[userId] = now;

        var photoUid = GetEntity(ev.PhotoEntity);

        if (!TryComp<STPhotoComponent>(photoUid, out var photo))
            return;

        if (photo.ImageData.Length == 0)
            return;

        if (photo.PhotoId != ev.PhotoId)
            return;

        RaiseNetworkEvent(new STPhotoResponseEvent
        {
            PhotoId = photo.PhotoId,
            ImageData = photo.ImageData,
        }, args.SenderSession);
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        _lastPhotoRequest.Remove(args.Player.UserId);
    }
}
