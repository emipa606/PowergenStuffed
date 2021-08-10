using RimWorld;
using UnityEngine;
using Verse;

namespace SmallSolarsStuffed
{
    [StaticConstructorOnStartup]
    public class CompPowerPlantMiniSolar : CompPowerPlant
    {
        private const float NightPower = 0f;

        private static readonly Vector2 BarSize = new Vector2(1.225f, 0.07f);

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
                        FullSunPower = 600f;
                        break;
                    case "Gold":
                        FullSunPower = 640f;
                        break;
                    case "Steel":
                        FullSunPower = 560f;
                        break;
                    case "Plasteel":
                        FullSunPower = 700f;
                        break;
                    case "Uranium":
                        FullSunPower = 740f;
                        break;
                    default:
                        FullSunPower = 560f;
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

                return (num - num2) / (float) num;
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
            r.margin = 0.08f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}