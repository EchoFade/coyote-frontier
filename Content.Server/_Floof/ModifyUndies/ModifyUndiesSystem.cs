using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Consent;
using Content.Shared.DoAfter;
using Content.Shared.FloofStation;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;


namespace Content.Server.FloofStation.ModifyUndies;


/// <summary>
/// This is a component that lets you show/hide specific underwear slots.
///
/// </summary>
public sealed class ModifyUndiesSystem : EntitySystem
{
    [Dependency] private readonly MarkingManager _markingManager = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedConsentSystem _consentSystem = default!;

    public static ProtoId<ConsentTogglePrototype> GenitalMarkingsConsent = "GenitalMarkings";

    public static readonly VerbCategory UndiesCat =
        new("verb-categories-undies", "/Textures/Interface/VerbIcons/undies.png");

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ModifyUndiesComponent, GetVerbsEvent<Verb>>(AddModifyUndiesVerb);
        SubscribeLocalEvent<ModifyUndiesComponent, ModifyUndiesDoAfterEvent>(ToggleUndies);
    }

    private void AddModifyUndiesVerb(EntityUid uid, ModifyUndiesComponent component, GetVerbsEvent<Verb> args)
    {
        if (args.Hands == null || !args.CanAccess || !args.CanInteract)
            return;
        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humApp))
            return;
        var isMine = args.User == args.Target;
        // okay go through their markings, and find all the undershirts and underwear markings
        // <marking_ID>, list:(localized name, bodypart enum, isvisible)
        foreach (var marking in humApp.MarkingSet.Markings.Values.SelectMany(markingLust => markingLust))
        {
            if (!_markingManager.TryGetMarking(marking, out var mProt))
                continue;
            // check if the Bodypart is in the component's BodyPartTargets
            if (!component.BodyPartTargets.Contains(mProt.BodyPart))
                continue;
                
            // Skip genital markings based on consent
            if (mProt.BodyPart == HumanoidVisualLayers.Genital)
            {
                // If user and target are the same person, they can always interact with their own markings
                if (args.User != args.Target)
                {
                    // For other players, only check the target's consent setting
                    var hasTargetConsent = _consentSystem.HasConsent(args.Target, GenitalMarkingsConsent);
                    if (!hasTargetConsent)
                    {
                        continue;
                    }
                }
            }
            
            var localizedName = Loc.GetString($"marking-{mProt.ID}");
            var partSlot = mProt.BodyPart;
            var isVisible = !humApp.HiddenMarkings.Contains(mProt.ID);
            if (mProt.Sprites.Count < 1)
                continue; // no sprites means its not visible means its kinda already off and you cant put it on
            var undieOrBra = partSlot switch
            {
                HumanoidVisualLayers.UndergarmentTop => new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/bra.png")),
                HumanoidVisualLayers.UndergarmentBottom => new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/underpants.png")),
                HumanoidVisualLayers.Genital => new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/love.png")),
                _ => new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/undies.png"))
            };
            // add the verb
            Verb verb = new()
            {
                Text = Loc.GetString(
                    "modify-undies-verb-text",
                    ("undies", localizedName),
                    ("isVisible", isVisible),
                    ("isMine", isMine),
                    ("target", Identity.Entity(args.Target, EntityManager))
                    ),
                Icon = undieOrBra,
                Category = UndiesCat,
                Act = () =>
                {
                    var ev = new ModifyUndiesDoAfterEvent(
                        marking,
                        localizedName,
                        isVisible
                        );
                    var doAfterArgs = new DoAfterArgs(
                        EntityManager,
                        args.User,
                        1f,
                        ev,
                        args.Target,
                        args.Target,
                        used: args.User
                    )
                    {
                        Hidden = false,
                        MovementThreshold = 0,
                        RequireCanInteract = true,
                        BlockDuplicate = true
                    };
                    string gString;
                    if (args.User == args.Target)
                    {
                        gString = isVisible
                            ? "undies-removed-self-start"
                            : "undies-equipped-self-start";
                        _popupSystem.PopupCoordinates(
                            Loc.GetString(
                                gString,
                                ("undie", localizedName)
                                ),
                            Transform(args.Target).Coordinates,
                            Filter.Entities(args.Target),
                            true,
                            PopupType.Medium);
                    }
                    // someone doing this to someone else
                    else
                    {
                        // to the user
                        gString = isVisible
                            ? "undies-removed-user-start"
                            : "undies-equipped-user-start";
                        _popupSystem.PopupCoordinates(
                            Loc.GetString(
                                gString,
                                ("undie", localizedName)
                                ),
                            Transform(args.Target).Coordinates,
                            Filter.Entities(args.User),
                            true,
                            PopupType.Medium);
                        // to the target
                        gString = isVisible
                            ? "undies-removed-target-start"
                            : "undies-equipped-target-start";
                        _popupSystem.PopupCoordinates(
                            Loc.GetString(
                                gString,
                                ("undie", localizedName),
                                ("user", Identity.Entity(args.User, EntityManager))
                                ),
                            Transform(args.Target).Coordinates,
                            Filter.Entities(args.Target),
                            true,
                            PopupType.MediumCaution);
                    }
                    // and then play a sound!
                    var rufthleAudio = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");
                    _audio.PlayEntity(
                        rufthleAudio,
                        Filter.Entities(args.User, args.Target),
                        args.Target,
                        false,
                        AudioParams.Default.WithVariation(2f).WithVolume(0.5f));
                    _doAfterSystem.TryStartDoAfter(doAfterArgs);
                },
                Disabled = false,
                Message = null
            };
            args.Verbs.Add(verb);
        }
    }
    private void ToggleUndies(
        EntityUid uid,
        ModifyUndiesComponent component,
        ModifyUndiesDoAfterEvent args
        )
    {
        if (!_markingManager.TryGetMarking(args.Marking, out var mProt))
            return;
        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humApp))
            return;

        _humanoid.SetMarkingVisibility(
            uid,
            humApp,
            mProt.ID,
            !args.IsVisible
        );
        // then make a text bubble!
        // one for the doer, one for the target
        // and one if the doer is the target
        // Effect targets for different players
        // Popups
        string gString;
        if (args.User == args.Target.Value)
        {
            gString = args.IsVisible
                ? "undies-removed-self"
                : "undies-equipped-self";
            _popupSystem.PopupCoordinates(
                Loc.GetString(
                    gString,
                    ("undie", args.MarkingPrototypeName)
                    ),
                Transform(args.Target.Value).Coordinates,
                Filter.Entities(args.Target.Value),
                true,
                PopupType.Medium);
        }
        // someone doing this to someone else
        else
        {
            // to the user
            gString = args.IsVisible
                ? "undies-removed-user"
                : "undies-equipped-user";
            _popupSystem.PopupCoordinates(
                Loc.GetString(
                    gString,
                    ("undie", args.MarkingPrototypeName)
                    ),
                Transform(args.Target.Value).Coordinates,
                Filter.Entities(args.User),
                true,
                PopupType.Medium);
            // to the target
            gString = args.IsVisible
                ? "undies-removed-target"
                : "undies-equipped-target";
            _popupSystem.PopupCoordinates(
                Loc.GetString(
                    gString,
                    ("undie", args.MarkingPrototypeName),
                    ("user", Identity.Entity(args.User, EntityManager))
                    ),
                Transform(args.Target.Value).Coordinates,
                Filter.Entities(args.Target.Value),
                true,
                PopupType.Medium);
        }
        // and then play a sound!
        var rufthleAudio = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");
        _audio.PlayEntity(
            rufthleAudio,
            Filter.Entities(args.User, args.Target.Value),
            args.Target.Value,
            false,
            AudioParams.Default.WithVariation(0.5f).WithVolume(0.5f));
    }
}
