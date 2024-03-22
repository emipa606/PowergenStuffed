using RimWorld;
using Verse;

namespace PowergenStuffed;

public class CompPowerPlantSteam : CompPowerPlant
{
    private Building_SteamGeyser geyser;

    private float steamPower;
    private IntermittentSteamSprayer steamSprayer;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        steamSprayer = new IntermittentSteamSprayer(parent);
        if (!parent.def.MadeFromStuff)
        {
            return;
        }

        switch (parent.Stuff.defName)
        {
            case "Silver":
                steamPower = 4200f;
                break;
            case "Gold":
                steamPower = 5000f;
                break;
            case "Steel":
                steamPower = 3600f;
                break;
            case "Plasteel":
                steamPower = 5200f;
                break;
            case "Uranium":
                steamPower = 6000f;
                break;
            default:
                steamPower = 3600f;
                break;
        }
    }

    public override void CompTick()
    {
        base.CompTick();
        if (geyser == null)
        {
            geyser = (Building_SteamGeyser)parent.Map.thingGrid.ThingAt(parent.Position, ThingDefOf.SteamGeyser);
        }

        if (geyser != null)
        {
            geyser.harvester = (Building)parent;
            steamSprayer.SteamSprayerTick();
        }

        PowerOutput = steamPower;
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        if (geyser != null)
        {
            geyser.harvester = null;
        }
    }
}