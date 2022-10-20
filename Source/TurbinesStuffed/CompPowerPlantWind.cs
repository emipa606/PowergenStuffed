using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TurbinesStuffed;

[StaticConstructorOnStartup]
public class CompPowerPlantWind : CompPowerPlant
{
    private const float MaxUsableWindIntensity = 1.5f;

    private const float SpinRateFactor = 0.05f;

    private const float PowerReductionPercentPerObstacle = 0.2f;

    private const string TranslateWindPathIsBlockedBy = "WindTurbine_WindPathIsBlockedBy";

    private const string TranslateWindPathIsBlockedByRoof = "WindTurbine_WindPathIsBlockedByRoof";

    private static readonly float BladeWidth = 6.6f;
    private static readonly float VerticalBladeOffset = 0.7f;
    private static readonly float HorizontalBladeOffset = -0.02f;

    private static Vector2 BarSize;

    private static readonly Material WindTurbineBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

    private static readonly Material WindTurbineBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

    private static readonly Material WindTurbineBladesMat =
        MaterialPool.MatFrom("Things/Building/Power/WindTurbine/WindTurbineBlades");

    private readonly List<Thing> windPathBlockedByThings = new List<Thing>();

    private readonly List<IntVec3> windPathBlockedCells = new List<IntVec3>();

    private readonly List<IntVec3> windPathCells = new List<IntVec3>();

    private float cachedPowerOutput;

    private float spinPosition;
    public float stuffFactor;

    private Sustainer sustainer;
    private int ticksSinceWeatherUpdate;

    public int updateWeatherEveryXTicks = 250;

    protected override float DesiredPowerOutput => cachedPowerOutput;

    public float PowerPercent => PowerOutput / (-Props.PowerConsumption * 1.5f * stuffFactor);

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        BarSize = new Vector2(parent.def.size.z - 0.95f, 0.14f);
        RecalculateBlockages();
        spinPosition = Rand.Range(0f, 15f);
        if (!parent.def.MadeFromStuff)
        {
            return;
        }

        switch (parent.Stuff.defName)
        {
            case "Silver":
                stuffFactor = 1.1f;
                break;
            case "Gold":
                stuffFactor = 1.2f;
                break;
            case "Steel":
                stuffFactor = 1f;
                break;
            case "Plasteel":
                stuffFactor = 1.4f;
                break;
            case "Uranium":
                stuffFactor = 1.5f;
                break;
            default:
                stuffFactor = 1f;
                break;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksSinceWeatherUpdate, "updateCounter");
        Scribe_Values.Look(ref cachedPowerOutput, "cachedPowerOutput");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (!PowerOn)
        {
            cachedPowerOutput = 0f;
            return;
        }

        ticksSinceWeatherUpdate++;
        if (ticksSinceWeatherUpdate >= updateWeatherEveryXTicks)
        {
            var num = Mathf.Min(parent.Map.windManager.WindSpeed, 1.5f);
            ticksSinceWeatherUpdate = 0;
            cachedPowerOutput = -(Props.PowerConsumption * num * stuffFactor);
            RecalculateBlockages();
            if (windPathBlockedCells.Count > 0)
            {
                var num2 = 0f;
                for (var i = 0; i < windPathBlockedCells.Count; i++)
                {
                    num2 += cachedPowerOutput * 0.2f;
                }

                cachedPowerOutput -= num2;
                if (cachedPowerOutput < 0f)
                {
                    cachedPowerOutput = 0f;
                }
            }
        }

        if (cachedPowerOutput > 0.01f)
        {
            spinPosition += PowerPercent * 0.05f;
        }

        if (sustainer == null || sustainer.Ended)
        {
            sustainer = SoundDefOf.WindTurbine_Ambience.TrySpawnSustainer(SoundInfo.InMap(parent));
        }

        sustainer.Maintain();
        sustainer.externalParams["PowerOutput"] = PowerPercent;
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var r = new GenDraw.FillableBarRequest
        {
            center = parent.DrawPos + (Vector3.up * 0.1f),
            size = BarSize,
            fillPercent = PowerPercent,
            filledMat = WindTurbineBarFilledMat,
            unfilledMat = WindTurbineBarUnfilledMat,
            margin = 0.15f
        };
        var rotation = parent.Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
        var vector = parent.TrueCenter();
        vector += parent.Rotation.FacingCell.ToVector3() * VerticalBladeOffset;
        vector += parent.Rotation.RighthandCell.ToVector3() * HorizontalBladeOffset;
        vector.y += 0.04054054f;
        var num = BladeWidth * Mathf.Sin(spinPosition);
        if (num < 0f)
        {
            num *= -1f;
        }

        var vector2 = new Vector2(num, 1f);
        var s = new Vector3(vector2.x, 1f, vector2.y);
        var matrix = default(Matrix4x4);
        matrix.SetTRS(vector, parent.Rotation.AsQuat, s);
        Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10 : MeshPool.plane10Flip,
            matrix, WindTurbineBladesMat, 0);
        vector.y -= 0.08108108f;
        matrix.SetTRS(vector, parent.Rotation.AsQuat, s);
        Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10Flip : MeshPool.plane10,
            matrix, WindTurbineBladesMat, 0);
    }

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.CompInspectStringExtra());
        if (windPathBlockedCells.Count <= 0)
        {
            return stringBuilder.ToString();
        }

        Thing thing = null;
        if (windPathBlockedByThings != null)
        {
            thing = windPathBlockedByThings[0];
        }

        if (thing != null)
        {
            stringBuilder.Append("WindTurbine_WindPathIsBlockedBy".Translate() + " " + thing.Label);
        }
        else
        {
            stringBuilder.Append("WindTurbine_WindPathIsBlockedByRoof".Translate());
        }

        return stringBuilder.ToString();
    }

    public override void PostDeSpawn(Map map)
    {
        base.PostDeSpawn(map);
        if (sustainer is { Ended: false })
        {
            sustainer.End();
        }
    }

    private void RecalculateBlockages()
    {
        if (windPathCells.Count == 0)
        {
            var collection =
                WindTurbineUtility.CalculateWindCells(parent.Position, parent.Rotation, parent.def.size);
            windPathCells.AddRange(collection);
        }

        windPathBlockedCells.Clear();
        windPathBlockedByThings.Clear();
        foreach (var intVec in windPathCells)
        {
            if (parent.Map.roofGrid.Roofed(intVec))
            {
                windPathBlockedByThings.Add(null);
                windPathBlockedCells.Add(intVec);
            }
            else
            {
                var list = parent.Map.thingGrid.ThingsListAt(intVec);
                foreach (var thing in list)
                {
                    if (!thing.def.blockWind)
                    {
                        continue;
                    }

                    windPathBlockedByThings.Add(thing);
                    windPathBlockedCells.Add(intVec);
                    break;
                }
            }
        }
    }
}