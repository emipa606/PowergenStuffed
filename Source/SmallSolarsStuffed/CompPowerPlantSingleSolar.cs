﻿using RimWorld;
using UnityEngine;
using Verse;

namespace PowergenStuffed;

[StaticConstructorOnStartup]
public class CompPowerPlantSingleSolar : CompPowerPlant
{
    private const float NightPower = 0f;

    private static readonly Vector2 BarSize = new(0.575f, 0.04f);

    private static readonly Material PowerPlantSolarBarFilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

    private static readonly Material PowerPlantSolarBarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

    private float FullSunPower;

    protected override float DesiredPowerOutput
    {
        get
        {
            if (!parent.def.MadeFromStuff)
            {
                return Mathf.Lerp(0f, FullSunPower, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;
            }

            switch (parent.Stuff.defName)
            {
                case "Silver":
                    FullSunPower = 145f;
                    break;
                case "Gold":
                    FullSunPower = 155f;
                    break;
                case "Steel":
                    FullSunPower = 135f;
                    break;
                case "Plasteel":
                    FullSunPower = 165f;
                    break;
                case "Uranium":
                    FullSunPower = 175f;
                    break;
                default:
                    FullSunPower = 145f;
                    break;
            }

            return Mathf.Lerp(0f, FullSunPower, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;
        }
    }

    private float RoofedPowerOutputFactor
    {
        get
        {
            var num = 0;
            var num2 = 0;
            foreach (var current in parent.OccupiedRect())
            {
                num++;
                if (parent.Map.roofGrid.Roofed(current))
                {
                    num2++;
                }
            }

            return (num - num2) / (float)num;
        }
    }

    public override void PostDraw()
    {
        base.PostDraw();
        var r = default(GenDraw.FillableBarRequest);
        r.center = parent.DrawPos + (Vector3.up * 0.1f);
        r.size = BarSize;
        r.fillPercent = PowerOutput / FullSunPower;
        r.filledMat = PowerPlantSolarBarFilledMat;
        r.unfilledMat = PowerPlantSolarBarUnfilledMat;
        r.margin = 0.05f;
        var rotation = parent.Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
    }
}