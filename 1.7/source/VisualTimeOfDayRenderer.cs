using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cheat_Menu
{
    /// <summary>
    /// Reproduces SkyManager's real render pipeline (weather sky-color blend, game conditions,
    /// weather events, CompAffectsSky) using a fixed apparent day fraction and glow instead of the
    /// real ones, then writes the same shared materials/shader globals SkyManager writes.
    /// Never touches SkyManager.CurSky/CurSkyGlow, GenCelestial, or any gameplay-facing state.
    /// </summary>
    public static class VisualTimeOfDayRenderer
    {
        private static readonly Color FogOfWarBaseColor = new Color32(77, 69, 66, byte.MaxValue);

        private static readonly float[] GlowThresholds = { 0f, 0.1f, 0.6f, 1f };

        private static readonly List<GameCondition> ScratchConditions = new List<GameCondition>();
        private static readonly List<Pair<SkyOverlay, float>> ScratchOverlays = new List<Pair<SkyOverlay, float>>();

        private static readonly int DayPercentShaderId = Shader.PropertyToID("_DayPercent");
        private static readonly int LightsourceShineIntensityShaderId = Shader.PropertyToID("_LightsourceShineIntensity");
        private static readonly int LightsourceShineSizeReductionShaderId = Shader.PropertyToID("_LightsourceShineSizeReduction");

        public static void Apply(Map map, float dayFraction, float glow)
        {
            SkyTarget visualSky = ComputeVisualSkyTarget(map, glow);

            MatBases.LightOverlay.color = visualSky.colors.sky;
            Find.CameraColor.saturation = visualSky.colors.saturation;

            Color fogColor = visualSky.colors.sky;
            fogColor.a = 1f;
            fogColor *= map.FogOfWarColor ?? FogOfWarBaseColor;
            MatBases.FogOfWar.color = fogColor;

            float visualShadowStrength = Mathf.Clamp01(Mathf.Abs(glow - 0.6f) / 0.15f);
            Color shadowColor = visualSky.colors.shadow;
            Vector2? overrideShadowVector = GetOverriddenShadowVector(map);
            Vector2 shadowVector;
            if (overrideShadowVector.HasValue)
            {
                shadowVector = overrideShadowVector.Value;
            }
            else
            {
                shadowVector = GetVisualLightSourceInfo(dayFraction, glow, GenCelestial.LightType.Shadow).vector;
                shadowColor = Color.Lerp(Color.white, shadowColor, visualShadowStrength);
            }

            Shader.SetGlobalVector(ShaderPropertyIDs.MapSunLightDirection, new Vector4(shadowVector.x, 0f, shadowVector.y, visualShadowStrength));

            GenCelestial.LightInfo sunInfo = GetVisualLightSourceInfo(dayFraction, glow, GenCelestial.LightType.LightingSun);
            GenCelestial.LightInfo moonInfo = GetVisualLightSourceInfo(dayFraction, glow, GenCelestial.LightType.LightingMoon);
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectSun, new Vector4(sunInfo.vector.x, 0f, sunInfo.vector.y, sunInfo.intensity));
            Shader.SetGlobalVector(ShaderPropertyIDs.WaterCastVectMoon, new Vector4(moonInfo.vector.x, 0f, moonInfo.vector.y, moonInfo.intensity));
            Shader.SetGlobalFloat(LightsourceShineSizeReductionShaderId, 20f * (1f / visualSky.lightsourceShineSize));
            Shader.SetGlobalFloat(LightsourceShineIntensityShaderId, visualSky.lightsourceShineIntensity);
            Shader.SetGlobalFloat(DayPercentShaderId, dayFraction);

            MatBases.SunShadow.color = shadowColor;
            MatBases.SunShadowFade.color = shadowColor;

            UpdateOverlays(map, visualSky);
        }

        private static SkyTarget ComputeVisualSkyTarget(Map map, float glow)
        {
            SkyTarget target = SkyTarget.Lerp(
                VisualSkyTargetForWeather(map.weatherManager.lastWeather, glow),
                VisualSkyTargetForWeather(map.weatherManager.curWeather, glow),
                map.weatherManager.TransitionLerpFactor);

            ScratchConditions.Clear();
            map.gameConditionManager.GetAllGameConditionsAffectingMap(map, ScratchConditions);
            for (int i = 0; i < ScratchConditions.Count; i++)
            {
                SkyTarget? conditionSky = ScratchConditions[i].SkyTarget(map);
                if (conditionSky.HasValue)
                {
                    target = SkyTarget.LerpDarken(target, conditionSky.Value, ScratchConditions[i].SkyTargetLerpFactor(map));
                }
            }
            ScratchConditions.Clear();

            List<WeatherEvent> liveEvents = map.weatherManager.eventHandler.LiveEventsListForReading;
            for (int i = 0; i < liveEvents.Count; i++)
            {
                if (liveEvents[i].CurrentlyAffectsSky)
                {
                    target = SkyTarget.Lerp(target, liveEvents[i].SkyTarget, liveEvents[i].SkyTargetLerpFactor);
                }
            }

            List<Thing> affectsSkyThings = map.listerThings.ThingsInGroup(ThingRequestGroup.AffectsSky);
            for (int i = 0; i < affectsSkyThings.Count; i++)
            {
                CompAffectsSky comp = affectsSkyThings[i].TryGetComp<CompAffectsSky>();
                if (comp.LerpFactor > 0f)
                {
                    target = comp.Props.lerpDarken
                        ? SkyTarget.LerpDarken(target, comp.SkyTarget, comp.LerpFactor)
                        : SkyTarget.Lerp(target, comp.SkyTarget, comp.LerpFactor);
                }
            }

            return target;
        }

        private static SkyTarget VisualSkyTargetForWeather(WeatherDef def, float glow)
        {
            int lowIndex = 0;
            int highIndex = 0;
            for (int i = 0; i < GlowThresholds.Length; i++)
            {
                highIndex = i;
                if (glow + 0.001f < GlowThresholds[i])
                {
                    break;
                }
                lowIndex = i;
            }

            float span = GlowThresholds[highIndex] - GlowThresholds[lowIndex];
            float t = span != 0f ? (glow - GlowThresholds[lowIndex]) / span : 1f;

            SkyTarget result = default;
            result.glow = Mathf.Min(glow, def.maxGlow);
            result.colors = SkyColorSet.Lerp(WeatherColorSetAt(def, lowIndex), WeatherColorSetAt(def, highIndex), t);

            bool daytime = GenCelestial.IsDaytime(glow);
            result.lightsourceShineIntensity = daytime ? 1f : 0.7f;
            result.lightsourceShineSize = daytime ? 1f : 0.5f;
            return result;
        }

        private static SkyColorSet WeatherColorSetAt(WeatherDef def, int index)
        {
            switch (index)
            {
                case 0:
                    return def.skyColorsNightMid;
                case 1:
                    return def.skyColorsNightEdge;
                case 2:
                    return def.skyColorsDusk;
                default:
                    return def.skyColorsDay;
            }
        }

        private static GenCelestial.LightInfo GetVisualLightSourceInfo(float dayFraction, float glow, GenCelestial.LightType type)
        {
            bool daytime;
            float intensity;
            switch (type)
            {
                case GenCelestial.LightType.Shadow:
                    daytime = GenCelestial.IsDaytime(glow);
                    intensity = Mathf.Clamp01(Mathf.Abs(glow - 0.6f) / 0.15f);
                    break;
                case GenCelestial.LightType.LightingSun:
                    daytime = true;
                    intensity = Mathf.Clamp01((glow - 0.6f + 0.2f) / 0.15f);
                    break;
                case GenCelestial.LightType.LightingMoon:
                    daytime = false;
                    intensity = Mathf.Clamp01((0f - (glow - 0.6f - 0.2f)) / 0.15f);
                    break;
                default:
                    daytime = true;
                    intensity = 0f;
                    break;
            }

            float t;
            float minAngle;
            float maxAngle;
            if (daytime)
            {
                t = dayFraction;
                minAngle = -1.5f;
                maxAngle = 15f;
            }
            else
            {
                t = dayFraction > 0.5f
                    ? Mathf.InverseLerp(0.5f, 1f, dayFraction) * 0.5f
                    : 0.5f + Mathf.InverseLerp(0f, 0.5f, dayFraction) * 0.5f;
                minAngle = -0.9f;
                maxAngle = 15f;
            }

            float x = Mathf.LerpUnclamped(0f - maxAngle, maxAngle, t);
            float y = minAngle - 2.5f * (x * x / 100f);
            return new GenCelestial.LightInfo
            {
                vector = new Vector2(x, y),
                intensity = intensity
            };
        }

        private static Vector2? GetOverriddenShadowVector(Map map)
        {
            List<WeatherEvent> liveEvents = map.weatherManager.eventHandler.LiveEventsListForReading;
            for (int i = 0; i < liveEvents.Count; i++)
            {
                Vector2? overrideVector = liveEvents[i].OverrideShadowVector;
                if (overrideVector.HasValue)
                {
                    return overrideVector;
                }
            }

            List<Thing> affectsSkyThings = map.listerThings.ThingsInGroup(ThingRequestGroup.AffectsSky);
            for (int i = 0; i < affectsSkyThings.Count; i++)
            {
                Vector2? overrideVector = affectsSkyThings[i].TryGetComp<CompAffectsSky>().OverrideShadowVector;
                if (overrideVector.HasValue)
                {
                    return overrideVector;
                }
            }

            return null;
        }

        private static void UpdateOverlays(Map map, SkyTarget visualSky)
        {
            ScratchOverlays.Clear();

            List<SkyOverlay> curOverlays = map.weatherManager.curWeather.Worker.overlays;
            for (int i = 0; i < curOverlays.Count; i++)
            {
                AddScratchOverlay(new Pair<SkyOverlay, float>(curOverlays[i], map.weatherManager.TransitionLerpFactor));
            }

            List<SkyOverlay> lastOverlays = map.weatherManager.lastWeather.Worker.overlays;
            for (int i = 0; i < lastOverlays.Count; i++)
            {
                AddScratchOverlay(new Pair<SkyOverlay, float>(lastOverlays[i], 1f - map.weatherManager.TransitionLerpFactor));
            }

            List<GameCondition> activeConditions = map.gameConditionManager.ActiveConditions;
            for (int i = 0; i < activeConditions.Count; i++)
            {
                List<SkyOverlay> conditionOverlays = activeConditions[i].SkyOverlays(map);
                if (conditionOverlays == null)
                {
                    continue;
                }

                for (int j = 0; j < conditionOverlays.Count; j++)
                {
                    AddScratchOverlay(new Pair<SkyOverlay, float>(conditionOverlays[j], activeConditions[i].SkyTargetLerpFactor(map)));
                }
            }

            for (int i = 0; i < ScratchOverlays.Count; i++)
            {
                Color overlayColor = ScratchOverlays[i].First.ForcedOverlayColor ?? visualSky.colors.overlay;
                overlayColor.a = ScratchOverlays[i].Second;
                ScratchOverlays[i].First.SetOverlayColor(overlayColor);
            }

            ScratchOverlays.Clear();
        }

        private static void AddScratchOverlay(Pair<SkyOverlay, float> pair)
        {
            for (int i = 0; i < ScratchOverlays.Count; i++)
            {
                if (ScratchOverlays[i].First == pair.First)
                {
                    ScratchOverlays[i] = new Pair<SkyOverlay, float>(ScratchOverlays[i].First, Mathf.Clamp01(ScratchOverlays[i].Second + pair.Second));
                    return;
                }
            }

            ScratchOverlays.Add(pair);
        }
    }
}
