using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Seasonality.Textures;
using UnityEngine;

namespace Seasonality.Managers;

public static class WeatherManager
{
    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetAvailableEnvironments))]
    private static class GetAvailableEnvironments_Patch
    {
        private static void Postfix(Heightmap.Biome biome, ref List<EnvEntry> __result)
        {
            if (SeasonalityPlugin._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
            if (biome is Heightmap.Biome.None or Heightmap.Biome.AshLands) return;
            List<EnvEntry> entries = GetEnvironments(biome);
            if (entries.Count <= 0) return;
            __result = entries;
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    private static class EnvMan_Awake_Patch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance) return;
            if (SeasonalityPlugin._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
            __instance.m_environmentDuration = SeasonalityPlugin._WeatherDuration.Value * 60;
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsCold))]
    private static class EnvMan_IsCold_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (SeasonalityPlugin._WinterAlwaysCold.Value is SeasonalityPlugin.Toggle.Off) return;
            if (SeasonalityPlugin._Season.Value is not SeasonalityPlugin.Season.Winter ) return;
            __result = true;
        }
    }

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.IsFreezing))]
    private static class EnvMan_IsFreezing_Patch
    {
        private static void Postfix(ref bool __result)
        {
            if (SeasonalityPlugin._WeatherFreezes.Value is SeasonalityPlugin.Toggle.On) return;
            if (SeasonalityPlugin._Season.Value is not SeasonalityPlugin.Season.Winter && !SeasonManager.m_fading) return;
            if (Player.m_localPlayer.GetCurrentBiome() is Heightmap.Biome.Mountain) return;
            __result = false;
        }
    }

    private static void UpdateRenderSettings()
    {
        RenderSettings.fog = false;
        RenderSettings.ambientIntensity = 0f;
        RenderSettings.fogDensity = 0f;
    }

    public static void OnWeatherDurationChange(object sender, EventArgs e)
    {
        if (sender is not ConfigEntry<int> config) return;
        if (!EnvMan.instance) return;
        if (SeasonalityPlugin._EnableWeather.Value is SeasonalityPlugin.Toggle.Off) return;
        EnvMan.instance.m_environmentDuration = config.Value * 60;
    }

    private static List<EnvEntry> GetEnvironments(Heightmap.Biome biome)
    {
        List<EnvEntry> output = new();
        if (!SeasonalityPlugin._WeatherConfigs.TryGetValue(SeasonalityPlugin._Season.Value, out Dictionary<Heightmap.Biome, ConfigEntry<string>> configs)) return output;
        if (!configs.TryGetValue(biome, out ConfigEntry<string> config)) return output;
        if (config.Value.IsNullOrWhiteSpace()) return output;
        string[] options = config.Value.Split(',');
        foreach (string option in options)
        {
            string[] values = option.Split(':');
            if (values.Length != 2) continue;
            string env = values[0];
            if (!float.TryParse(values[1], out float weight)) continue;
            var envSetup = EnvMan.instance.m_environments.Find(x => x.m_name == env);
            if (envSetup == null) continue;
            var entry = new EnvEntry()
            {
                m_environment = env,
                m_weight = weight,
                m_env = envSetup
            };
            output.Add(entry);
        }
        return output;
    }
    

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class ObjectDB_Awake_Patch
    {
        private static void Postfix() => RegisterWeatherManSE();
    }

    private static void RegisterWeatherManSE()
    {
        if (!ObjectDB.instance || !ZNetScene.instance) return;
        SE_WeatherMan effect = ScriptableObject.CreateInstance<SE_WeatherMan>();
        effect.name = "SE_Weatherman";
        effect.m_name = "Weatherman";
        effect.m_icon = SpriteManager.ValknutIcon;
        if (ObjectDB.instance.m_StatusEffects.Contains(effect)) return;
        ObjectDB.instance.m_StatusEffects.Add(effect);
    }

    public static void OnDisplayConfigChange(object sender, EventArgs e)
    {
        if (sender is not ConfigEntry<SeasonalityPlugin.Toggle> config) return;
        if (config.Value is not SeasonalityPlugin.Toggle.On) return;
        Player.m_localPlayer.GetSEMan().RemoveStatusEffect("SE_Weatherman".GetStableHashCode());
        Player.m_localPlayer.GetSEMan().AddStatusEffect("SE_Weatherman".GetStableHashCode());
    }

    public class SE_WeatherMan : StatusEffect
    {
        public override string GetTooltipString()
        {
            return m_tooltip + Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}_tooltip");
        }

        public override string GetIconText() => GetEnvironmentCountDown();

        private string GetEnvironmentCountDown()
        {
            if (EnvMan.instance == null) return "";

            m_name = Localization.instance.Localize($"$weather_{EnvMan.instance.m_currentEnv.m_name.ToLower().Replace(" ", "_")}");
            m_icon = SeasonalityPlugin._DisplayWeather.Value is SeasonalityPlugin.Toggle.On && SeasonalityPlugin._EnableWeather.Value is SeasonalityPlugin.Toggle.On
                ? SpriteManager.ValknutIcon
                : null;

            if (SeasonalityPlugin._DisplayWeatherTimer.Value is SeasonalityPlugin.Toggle.Off) return "";
            double seed = EnvMan.instance.m_totalSeconds / EnvMan.instance.m_environmentDuration;
            double fraction = 1 - (seed - Math.Truncate(seed));
            double total = fraction * EnvMan.instance.m_environmentDuration;

            int hours = TimeSpan.FromSeconds(total).Hours;
            int minutes = TimeSpan.FromSeconds(total).Minutes;
            int seconds = TimeSpan.FromSeconds(total).Seconds;
    
            if (total < 0) return "";

            return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
        }
    }
}