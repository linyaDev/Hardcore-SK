using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace SK
{
    public class WeaponGizmoIcons : DefModExtension
    {
        public string meleeIconPath;
        public string rangedIconPath;

        [Unsaved] private Texture2D meleeIconCached;
        [Unsaved] private Texture2D rangedIconCached;

        public Texture2D MeleeIcon =>
            meleeIconCached ?? (meleeIconCached = ContentFinder<Texture2D>.Get(meleeIconPath, false));

        public Texture2D RangedIcon =>
            rangedIconCached ?? (rangedIconCached = ContentFinder<Texture2D>.Get(rangedIconPath, false));
    }

    [StaticConstructorOnStartup]
    public static class WeaponGizmoIconsPatches
    {
        static WeaponGizmoIconsPatches()
        {
            var harmony = new Harmony("sk.weapongizmoicons");

            var meleeGizmo = AccessTools.Method(typeof(PawnAttackGizmoUtility), "GetMeleeAttackGizmo");
            if (meleeGizmo != null)
            {
                harmony.Patch(meleeGizmo,
                    postfix: new HarmonyMethod(typeof(WeaponGizmoIconsPatches), nameof(MeleeGizmoPostfix)));
            }

            var verbCommand = AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand");
            if (verbCommand != null)
            {
                harmony.Patch(verbCommand,
                    postfix: new HarmonyMethod(typeof(WeaponGizmoIconsPatches), nameof(VerbCommandPostfix)));
            }

            Log.Message("[WeaponGizmoIcons] Patches applied.");
        }

        // Melee gizmo: fist icon -> custom icon
        public static void MeleeGizmoPostfix(ref Gizmo __result, Pawn pawn)
        {
            var primary = pawn.equipment?.Primary;
            if (primary == null)
                return;

            var ext = primary.def.GetModExtension<WeaponGizmoIcons>();
            if (ext?.MeleeIcon == null)
                return;

            if (__result is Command cmd)
            {
                cmd.icon = ext.MeleeIcon;
            }
        }

        // Verb gizmo: weapon texture -> custom icon (for ranged verbs)
        public static void VerbCommandPostfix(ref Command_VerbTarget __result, Thing ownerThing, Verb verb)
        {
            if (verb == null || verb.verbProps.IsMeleeAttack)
                return;

            var ext = ownerThing?.def.GetModExtension<WeaponGizmoIcons>();
            if (ext?.RangedIcon == null)
                return;

            __result.icon = ext.RangedIcon;
            var ownerField = AccessTools.Field(typeof(Command_VerbTarget), "ownerThing");
            ownerField?.SetValue(__result, null);
        }
    }
}
