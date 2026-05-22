using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace SK
{
    public class BrawlerFriendly : DefModExtension { }

    [StaticConstructorOnStartup]
    public static class BrawlerFriendlyPatches
    {
        static BrawlerFriendlyPatches()
        {
            var harmony = new Harmony("sk.brawlerfriendly");

            var thought = AccessTools.Method(typeof(ThoughtWorker_IsCarryingRangedWeapon), "CurrentStateInternal");
            if (thought != null)
            {
                harmony.Patch(thought,
                    postfix: new HarmonyMethod(typeof(BrawlerFriendlyPatches), nameof(ThoughtPostfix)));
            }

            var alert = AccessTools.PropertyGetter(typeof(Alert_BrawlerHasRangedWeapon), "BrawlersWithRangedWeapon");
            if (alert != null)
            {
                harmony.Patch(alert,
                    postfix: new HarmonyMethod(typeof(BrawlerFriendlyPatches), nameof(AlertPostfix)));
            }

            int count = DefDatabase<ThingDef>.AllDefs.Count(d => d.HasModExtension<BrawlerFriendly>());
            Log.Message($"[BrawlerFriendly] Patches applied. {count} weapon(s) marked as brawler-friendly.");
        }

        public static void ThoughtPostfix(ref ThoughtState __result, Pawn p)
        {
            if (!__result.Active)
                return;

            var primary = p.equipment?.Primary;
            if (primary != null && primary.def.HasModExtension<BrawlerFriendly>())
            {
                __result = ThoughtState.Inactive;
            }
        }

        public static void AlertPostfix(ref List<Pawn> __result)
        {
            __result.RemoveAll(p =>
            {
                var primary = p.equipment?.Primary;
                return primary != null && primary.def.HasModExtension<BrawlerFriendly>();
            });
        }
    }
}
