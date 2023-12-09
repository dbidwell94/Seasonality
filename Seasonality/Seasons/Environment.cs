﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using static Seasonality.SeasonalityPlugin;

namespace Seasonality.Seasons;

public static class Environment
{
    private static readonly CustomSyncedValue<string> SyncedWeatherData = new(SeasonalityPlugin.ConfigSync, "ServerWeather", "");
    private static void UpdateServerWeatherMan()
    {
        ISerializer serializer = new SerializerBuilder().Build();
        string data = serializer.Serialize(ServerWeatherIndexes);
        SyncedWeatherData.Value = data;
    }
    private static int GetServerWeatherManIndex(Heightmap.Biome land)
    {
        if (SyncedWeatherData.Value == "") return 0;
        
        IDeserializer deserializer = new DeserializerBuilder().Build();
        Dictionary<Heightmap.Biome, int> data = deserializer.Deserialize<Dictionary<Heightmap.Biome, int>>(SyncedWeatherData.Value);

        return data.TryGetValue(land, out int index) ? index : 0;
    }
    private static string GetEnvironmentName(Environments options)
    {
        return options switch
        {
            Environments.None => "",
            Environments.Clear => "Clear",
            Environments.Misty => "Misty",
            Environments.Darklands_dark => "Darklands_dark",
            Environments.HeathClear => "Heath clear",
            Environments.DeepForestMist => "DeepForest Mist",
            Environments.GDKing => "GDKing",
            Environments.Rain => "Rain",
            Environments.LightRain => "LightRain",
            Environments.ThunderStorm => "ThunderStorm",
            Environments.Eikthyr => "Eikthyr",
            Environments.GoblinKing => "GoblinKing",
            Environments.nofogts => "nofogts",
            Environments.SwampRain => "SwampRain",
            Environments.Bonemass => "Bonemass",
            Environments.Snow => "Snow",
            Environments.Twilight_Clear => "Twilight_Clear",
            Environments.Twilight_Snow => "Twilight_Snow",
            Environments.Twilight_SnowStorm => "Twilight_SnowStorm",
            Environments.SnowStorm => "SnowStorm",
            Environments.Moder => "Moder",
            Environments.Ashrain => "AshRain",
            Environments.Crypt => "Crypt",
            Environments.SunkenCrypt => "SunkenCrypt",
            Environments.Caves => "Caves",
            Environments.Mistlands_clear => "Mistlands_clear",
            Environments.Mistlands_rain => "Mistlands_rain",
            Environments.Mistlands_thunder => "Mistlands_thunder",
            Environments.InfectedMine => "InfectedMine",
            Environments.Queen => "Queen",
            Environments.WarmSnow => "WarmSnow",
            Environments.ClearWarmSnow => "ClearWarmSnow",
            Environments.NightFrost => "NightFrost",
            _ => ""
        };
    }
    public enum Environments
    {
        None,
        Clear,
        Twilight_Clear,
        Misty,
        Darklands_dark,
        HeathClear,
        DeepForestMist,
        GDKing,
        Rain,
        LightRain,
        ThunderStorm,
        Eikthyr,
        GoblinKing,
        nofogts,
        SwampRain,
        Bonemass,
        Snow,
        Twilight_Snow,
        Twilight_SnowStorm,
        SnowStorm,
        Moder,
        Ashrain,
        Crypt,
        SunkenCrypt,
        Caves,
        Mistlands_clear,
        Mistlands_rain,
        Mistlands_thunder,
        InfectedMine,
        Queen,
        WarmSnow,
        ClearWarmSnow,
        NightFrost,
    }
    
    private static readonly Dictionary<Heightmap.Biome, List<EnvEntry>> ServerWeatherMap = new();
    private static readonly Dictionary<Heightmap.Biome, int> ServerWeatherIndexes = new ();

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
    static class EnvManAwakePatch
    {
        private static void Postfix(EnvMan __instance)
        {
            if (!__instance) return;
            
            EnvSetup WarmSnow = CloneEnvSetup(__instance, "Snow", "WarmSnow");
            WarmSnow.m_isFreezing = false;
            WarmSnow.m_isFreezingAtNight = false;
            WarmSnow.m_isCold = true;
            WarmSnow.m_isColdAtNight = true;
            WarmSnow.m_lightIntensityDay = 0.6f;

            EnvSetup ClearWarmSnow = CloneEnvSetup(__instance, "Snow", "ClearWarmSnow");
            ClearWarmSnow.m_isFreezing = false;
            ClearWarmSnow.m_isFreezingAtNight = false;
            ClearWarmSnow.m_isCold = true;
            ClearWarmSnow.m_isColdAtNight = true;
            ClearWarmSnow.m_fogDensityMorning = 0.00f;
            ClearWarmSnow.m_fogDensityDay = 0.00f;
            ClearWarmSnow.m_fogDensityEvening = 0.00f;
            ClearWarmSnow.m_fogDensityNight = 0.00f;
            ClearWarmSnow.m_lightIntensityDay = 0.6f;

            EnvSetup NightFrost = CloneEnvSetup(__instance, "Snow", "NightFrost");
            ClearWarmSnow.m_isFreezing = false;
            ClearWarmSnow.m_isFreezingAtNight = true;
            ClearWarmSnow.m_isCold = true;
            ClearWarmSnow.m_isColdAtNight = false;
            ClearWarmSnow.m_fogDensityMorning = 0.00f;
            ClearWarmSnow.m_fogDensityDay = 0.00f;
            ClearWarmSnow.m_fogDensityEvening = 0.00f;
            ClearWarmSnow.m_fogDensityNight = 0.00f;
            ClearWarmSnow.m_lightIntensityDay = 0.6f;
            
            __instance.m_environments.Add(NightFrost);
            __instance.m_environments.Add(ClearWarmSnow);
            __instance.m_environments.Add(WarmSnow);
        }
        private static EnvSetup CloneEnvSetup(EnvMan __instance, string originalName, string newName)
        {
            EnvSetup originalSetup = __instance.m_environments.Find(x => x.m_name == originalName);
            EnvSetup newSetup = new EnvSetup()
            {
                m_name = newName,
                m_default = originalSetup.m_default, // Means enabled/disabled
                m_isWet = originalSetup.m_isWet,
                m_isFreezing = originalSetup.m_isFreezing,
                m_isFreezingAtNight = originalSetup.m_isFreezingAtNight,
                m_isCold = originalSetup.m_isCold,
                m_isColdAtNight = originalSetup.m_isColdAtNight,
                m_alwaysDark = originalSetup.m_alwaysDark,
                m_ambColorNight = originalSetup.m_ambColorNight,
                m_ambColorDay = originalSetup.m_ambColorDay,
                m_fogColorNight = originalSetup.m_fogColorNight,
                m_fogColorMorning = originalSetup.m_fogColorMorning,
                m_fogColorEvening = originalSetup.m_fogColorEvening,
                m_fogColorSunNight = originalSetup.m_fogColorSunNight,
                m_fogColorSunMorning = originalSetup.m_fogColorSunMorning,
                m_fogDensityNight = originalSetup.m_fogDensityNight,
                m_fogDensityMorning = originalSetup.m_fogDensityMorning,
                m_fogDensityEvening = originalSetup.m_fogDensityEvening,
                m_sunColorNight = originalSetup.m_sunColorNight,
                m_sunColorMorning = originalSetup.m_fogColorSunMorning,
                m_sunColorDay = originalSetup.m_sunColorDay,
                m_sunColorEvening = originalSetup.m_sunColorEvening,
                m_lightIntensityDay = originalSetup.m_lightIntensityDay,
                m_sunAngle = originalSetup.m_sunAngle,
                m_windMin = originalSetup.m_windMin,
                m_windMax = originalSetup.m_windMax,
                m_envObject = originalSetup.m_envObject,
                m_psystems = originalSetup.m_psystems,
                m_psystemsOutsideOnly = originalSetup.m_psystemsOutsideOnly,
                m_rainCloudAlpha = originalSetup.m_rainCloudAlpha,
                m_ambientLoop = originalSetup.m_ambientLoop,
                m_ambientVol = originalSetup.m_ambientVol,
                m_ambientList = originalSetup.m_ambientList,
                m_musicMorning = originalSetup.m_musicMorning,
                m_musicEvening = originalSetup.m_musicEvening,
                m_musicDay = originalSetup.m_musicDay,
                m_musicNight = originalSetup.m_musicNight
            };

            return newSetup;
        }
    }
    private static void AddToEntries(List<Environments> environments, List<EnvEntry> entries)
    {
        foreach (Environments value in environments)
        {
            if (value is Environments.None) continue;
            EnvEntry entry = new EnvEntry()
            {
                m_environment = GetEnvironmentName(value),
                m_weight = 1f
            };
            entries.Add(entry);
        }
    }
    private static string GetEnvironmentTooltip(string environment)
        {
            return (environment) switch
            {
                "Clear" => "The weather is clear and peaceful",
                "Misty" => "The gods do not engage with visionaries",
                "Darklands_dark" => "A true warrior is never afraid of the dark",
                "Heath clear" => "The weather is clear yet dangerous",
                "DeepForest Mist" => "Be careful of the deep forest mist",
                "GDKing" => "The energy of the forest swells into the sky",
                "Rain" => "A great day to stay indoors",
                "LightRain" => "Time to hunt the necks",
                "ThunderStorm" => "The gods are enraged",
                "Eikthyr" => "The might of Eikthyr thunders through the air",
                "GoblinKing" => "The air is charged with the king's energy",
                "nofogts" => "The rain hits different these days",
                "SwampRain" => "There is a hint of acidity to these rain drops",
                "Bonemass" => "The gaseous might of the swamps is palpable",
                "Snow" => "The air is cold and frigid",
                "Twilight_Clear" => "The calm before the storm",
                "Twilight_Snow" => "Beautiful comes in a multitude of shapes",
                "Twilight_SnowStorm" => "The frozen sky is cold and terrifying",
                "SnowStorm" => "A true viking knows how to navigate through the harshest of weathers",
                "Moder" => "Something powerful is soaring through the frigid skies",
                "AshRain" => "Not even Hela can withstand the burning temperature",
                "Crypt" => "Eerie times calls for tempered measures",
                "SunkenCrypts" => "The sound of gutters radiate through the land",
                "Caves" => "The shallow bowels of the mountains calls for you",
                "Mistlands_clear" => "The weather clears, yet the dread increases",
                "Mistlands_rain" => "The gods cry upon your arrival",
                "Mistlands_thunder" => "The sky breaks open to reveal magical moments",
                "InfectedMine" => "The time to fret was long ago",
                "Queen" => "This is when you display your bravery",
                "WarmSnow" => "Bright and warm, the air still showers you with snow",
                "ClearWarmSnow" => "A moment of peaceful tranquility is all one needs",
                "NightFrost" => "The nights are frozen with trepidation",
                _ => ""
            };
        }
    private static string GetEnvironmentDisplayName(string environment)
    {
        return (environment) switch
        {
            "Darklands_dark" => "Darklands Dark",
            "Heath clear" => "Heath Clear",
            "GDKing" => "Forest King",
            "LightRain" => "Light Rain",
            "ThunderStorm" => "Thunder Storm",
            "GoblinKing" => "Goblin King",
            "nofogts" => "Clear Thunder Storm",
            "SwampRain" => "Swamp Rain",
            "Bonemass" => "Swamp King",
            "Twilight_Clear" => "Twilight Clear",
            "Twilight_Snow" => "Twilight Snow",
            "Twilight_SnowStorm" => "Twilight Snow Storm",
            "SnowStorm" => "Snow Storm",
            "Moder" => "Dragon Queen",
            "AshRain" => "Ash Rain",
            "SunkenCrypts" => "Sunken Crypt",
            "Mistlands_clear" => "Mistland Clear",
            "Mistlands_rain" => "Mistland Rain",
            "Mistlands_thunder" => "Mistland Thunder",
            "InfectedMine" => "Infected Mine",
            "Queen" => "Seeker Queen",
            "WarmSnow" => "Fog Snow",
            "ClearWarmSnow" => "Clear Snow",
            "NightFrost" => "Night Frost",
            _ => environment
        };
    }
    private static void SetWeatherMan(string env)
    {
        if (!Player.m_localPlayer) return;
        
        EnvironmentEffectData EnvData = new EnvironmentEffectData()
        {
            name = "WeatherMan_SE",
            m_name = GetEnvironmentDisplayName(env),
            m_sprite = CustomTextures.ValknutIcon,
            m_start_msg = "The weather is changing to " + GetEnvironmentDisplayName(env),
            m_tooltip = GetEnvironmentTooltip(env)
        };
        Player.m_localPlayer.GetSEMan().RemoveStatusEffect("WeatherMan_SE".GetStableHashCode());
        StatusEffect WeatherEffect = EnvData.InitEnvEffect();
        Player.m_localPlayer.GetSEMan().AddStatusEffect(WeatherEffect);
    }

    private static Heightmap.Biome lastBiome = Heightmap.Biome.None;
    private static string currentEnv = null!;
    private static bool WeatherTweaked;

    private static int MeadowIndex;
    private static int BlackForestIndex;
    private static int SwampIndex;
    private static int MountainIndex;
    private static int PlainsIndex;
    private static int MistLandsIndex;
    private static int AshLandsIndex;
    private static int DeepNorthIndex;
    private static int OceanIndex;

    [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateEnvironment))]
    static class EnvManPatch
    {
        private static bool Prefix(EnvMan __instance, long sec, Heightmap.Biome biome)
        {
            if (_ModEnabled.Value is Toggle.Off || _WeatherControl.Value is Toggle.Off)
            {
                if (!Player.m_localPlayer) return true;
                Player.m_localPlayer.GetSEMan().RemoveStatusEffect("WeatherMan_SE".GetStableHashCode());
                return true;
            }
            
            if (workingAsType is WorkingAs.Server)
            {
                ServerSyncedWeatherMan(__instance);
                return false;
            }
            
            // If client is overriding weather system
            string environmentOverride = __instance.GetEnvironmentOverride();
            if (!string.IsNullOrEmpty(environmentOverride))
            {
                // If debug mode is active and user forces environment
                __instance.m_environmentPeriod = -1L;
                __instance.m_currentBiome = __instance.GetBiome();
                __instance.QueueEnvironment(environmentOverride);
                if (__instance.m_currentEnv.m_name == currentEnv) return false;
                SetWeatherMan(environmentOverride);
                currentEnv = __instance.m_currentEnv.m_name;
                WeatherTweaked = false;
                return false;
            }

            if (!Player.m_localPlayer) return true;
            if (Player.m_localPlayer.IsDead()) return true;
            
            Heightmap.Biome currentBiome = Heightmap.FindBiome(Player.m_localPlayer.transform.position);
            if (currentBiome == Heightmap.Biome.None) return false;

            List<EnvEntry> entries = new();
            List<Environments> configs = new();
            
            switch (currentBiome)
            {
                case Heightmap.Biome.Meadows:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Meadows_Weather1.Value);
                            configs.Add(_Winter_Meadows_Weather2.Value);
                            configs.Add(_Winter_Meadows_Weather3.Value);
                            configs.Add(_Winter_Meadows_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Meadows_Weather1.Value);
                            configs.Add(_Fall_Meadows_Weather2.Value);
                            configs.Add(_Fall_Meadows_Weather3.Value);
                            configs.Add(_Fall_Meadows_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Meadows_Weather1.Value);
                            configs.Add(_Spring_Meadows_Weather2.Value);
                            configs.Add(_Spring_Meadows_Weather3.Value);
                            configs.Add(_Spring_Meadows_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Meadows_Weather1.Value);
                            configs.Add(_Summer_Meadows_Weather2.Value);
                            configs.Add(_Summer_Meadows_Weather3.Value);
                            configs.Add(_Summer_Meadows_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.BlackForest:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_BlackForest_Weather1.Value);
                            configs.Add(_Winter_BlackForest_Weather2.Value);
                            configs.Add(_Winter_BlackForest_Weather3.Value);
                            configs.Add(_Winter_BlackForest_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_BlackForest_Weather1.Value);
                            configs.Add(_Fall_BlackForest_Weather2.Value);
                            configs.Add(_Fall_BlackForest_Weather3.Value);
                            configs.Add(_Fall_BlackForest_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_BlackForest_Weather1.Value);
                            configs.Add(_Spring_BlackForest_Weather2.Value);
                            configs.Add(_Spring_BlackForest_Weather3.Value);
                            configs.Add(_Spring_BlackForest_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_BlackForest_Weather1.Value);
                            configs.Add(_Summer_BlackForest_Weather2.Value);
                            configs.Add(_Summer_BlackForest_Weather3.Value);
                            configs.Add(_Summer_BlackForest_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Swamp:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Swamp_Weather1.Value);
                            configs.Add(_Winter_Swamp_Weather2.Value);
                            configs.Add(_Winter_Swamp_Weather3.Value);
                            configs.Add(_Winter_Swamp_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Swamp_Weather1.Value);
                            configs.Add(_Fall_Swamp_Weather2.Value);
                            configs.Add(_Fall_Swamp_Weather3.Value);
                            configs.Add(_Fall_Swamp_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Swamp_Weather1.Value);
                            configs.Add(_Spring_Swamp_Weather2.Value);
                            configs.Add(_Spring_Swamp_Weather3.Value);
                            configs.Add(_Spring_Swamp_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Swamp_Weather1.Value);
                            configs.Add(_Summer_Swamp_Weather2.Value);
                            configs.Add(_Summer_Swamp_Weather3.Value);
                            configs.Add(_Summer_Swamp_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mountain:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Mountains_Weather1.Value);
                            configs.Add(_Winter_Mountains_Weather2.Value);
                            configs.Add(_Winter_Mountains_Weather3.Value);
                            configs.Add(_Winter_Mountains_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Mountains_Weather1.Value);
                            configs.Add(_Fall_Mountains_Weather2.Value);
                            configs.Add(_Fall_Mountains_Weather3.Value);
                            configs.Add(_Fall_Mountains_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Mountains_Weather1.Value);
                            configs.Add(_Spring_Mountains_Weather2.Value);
                            configs.Add(_Spring_Mountains_Weather3.Value);
                            configs.Add(_Spring_Mountains_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Mountains_Weather1.Value);
                            configs.Add(_Summer_Mountains_Weather2.Value);
                            configs.Add(_Summer_Mountains_Weather3.Value);
                            configs.Add(_Summer_Mountains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Plains:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Plains_Weather1.Value);
                            configs.Add(_Winter_Plains_Weather2.Value);
                            configs.Add(_Winter_Plains_Weather3.Value);
                            configs.Add(_Winter_Plains_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Plains_Weather1.Value);
                            configs.Add(_Fall_Plains_Weather2.Value);
                            configs.Add(_Fall_Plains_Weather3.Value);
                            configs.Add(_Fall_Plains_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Plains_Weather1.Value);
                            configs.Add(_Spring_Plains_Weather2.Value);
                            configs.Add(_Spring_Plains_Weather3.Value);
                            configs.Add(_Spring_Plains_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Plains_Weather1.Value);
                            configs.Add(_Summer_Plains_Weather2.Value);
                            configs.Add(_Summer_Plains_Weather3.Value);
                            configs.Add(_Summer_Plains_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Mistlands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_MistLands_Weather1.Value);
                            configs.Add(_Winter_MistLands_Weather2.Value);
                            configs.Add(_Winter_MistLands_Weather3.Value);
                            configs.Add(_Winter_MistLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_MistLands_Weather1.Value);
                            configs.Add(_Fall_MistLands_Weather2.Value);
                            configs.Add(_Fall_MistLands_Weather3.Value);
                            configs.Add(_Fall_MistLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_MistLands_Weather1.Value);
                            configs.Add(_Spring_MistLands_Weather2.Value);
                            configs.Add(_Spring_MistLands_Weather3.Value);
                            configs.Add(_Spring_MistLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_MistLands_Weather1.Value);
                            configs.Add(_Summer_MistLands_Weather2.Value);
                            configs.Add(_Summer_MistLands_Weather3.Value);
                            configs.Add(_Summer_MistLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.Ocean:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_Ocean_Weather1.Value);
                            configs.Add(_Winter_Ocean_Weather2.Value);
                            configs.Add(_Winter_Ocean_Weather3.Value);
                            configs.Add(_Winter_Ocean_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_Ocean_Weather1.Value);
                            configs.Add(_Fall_Ocean_Weather2.Value);
                            configs.Add(_Fall_Ocean_Weather3.Value);
                            configs.Add(_Fall_Ocean_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_Ocean_Weather1.Value);
                            configs.Add(_Spring_Ocean_Weather2.Value);
                            configs.Add(_Spring_Ocean_Weather3.Value);
                            configs.Add(_Spring_Ocean_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_Ocean_Weather1.Value);
                            configs.Add(_Summer_Ocean_Weather2.Value);
                            configs.Add(_Summer_Ocean_Weather3.Value);
                            configs.Add(_Summer_Ocean_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.AshLands:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_AshLands_Weather1.Value);
                            configs.Add(_Winter_AshLands_Weather2.Value);
                            configs.Add(_Winter_AshLands_Weather3.Value);
                            configs.Add(_Winter_AshLands_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_AshLands_Weather1.Value);
                            configs.Add(_Fall_AshLands_Weather2.Value);
                            configs.Add(_Fall_AshLands_Weather3.Value);
                            configs.Add(_Fall_AshLands_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_AshLands_Weather1.Value);
                            configs.Add(_Spring_AshLands_Weather2.Value);
                            configs.Add(_Spring_AshLands_Weather3.Value);
                            configs.Add(_Spring_AshLands_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_AshLands_Weather1.Value);
                            configs.Add(_Summer_AshLands_Weather2.Value);
                            configs.Add(_Summer_AshLands_Weather3.Value);
                            configs.Add(_Summer_AshLands_Weather4.Value);
                            break;
                    }
                    break;
                case Heightmap.Biome.DeepNorth:
                    switch (_Season.Value)
                    {
                        case Season.Winter:
                            configs.Add(_Winter_DeepNorth_Weather1.Value);
                            configs.Add(_Winter_DeepNorth_Weather2.Value);
                            configs.Add(_Winter_DeepNorth_Weather3.Value);
                            configs.Add(_Winter_DeepNorth_Weather4.Value);
                            break;
                        case Season.Fall:
                            configs.Add(_Fall_DeepNorth_Weather1.Value);
                            configs.Add(_Fall_DeepNorth_Weather2.Value);
                            configs.Add(_Fall_DeepNorth_Weather3.Value);
                            configs.Add(_Fall_DeepNorth_Weather4.Value);
                            break;
                        case Season.Spring:
                            configs.Add(_Spring_DeepNorth_Weather1.Value);
                            configs.Add(_Spring_DeepNorth_Weather2.Value);
                            configs.Add(_Spring_DeepNorth_Weather3.Value);
                            configs.Add(_Spring_DeepNorth_Weather4.Value);
                            break;
                        case Season.Summer:
                            configs.Add(_Summer_DeepNorth_Weather1.Value);
                            configs.Add(_Summer_DeepNorth_Weather2.Value);
                            configs.Add(_Summer_DeepNorth_Weather3.Value);
                            configs.Add(_Summer_DeepNorth_Weather4.Value);
                            break;
                    }
                    break;
            }

            if (configs.TrueForAll(x => x is Environments.None))
            {
                if (__instance.m_currentEnv.m_name == currentEnv) return true;
                SetWeatherMan(__instance.m_currentEnv.m_name);
                currentEnv = __instance.m_currentEnv.m_name;
                WeatherTweaked = false;
                return true;
            }

            AddToEntries(configs, entries);

            // If client is server
            if (ZNet.instance.IsServer()) return LocalWeatherMan(__instance, sec, entries, currentBiome);
            
            // If client is connected to server, then use server index
            if (lastBiome != currentBiome)
            {
                ServerSyncedChangeWeather(currentBiome, __instance, entries, sec, false);
                lastBiome = currentBiome;
            }
            
            long duration = _WeatherDuration.Value * 60; // Total seconds

            if (duration == 0)
            {
                // Throttle weather change to a minimum of 1 minute
                if ((lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds > 0) return false;
                ServerSyncedChangeWeather(currentBiome, __instance, entries, sec);
            }
            else
            {
                if ((lastEnvironmentChange + _WeatherDuration.Value * 60) - EnvMan.instance.m_totalSeconds > 0) return false;
                ServerSyncedChangeWeather(currentBiome, __instance, entries, sec);
            }
            return false;

        }
        private static void ServerSyncedWeatherMan(EnvMan __instance)
        {
            foreach (Heightmap.Biome land in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                if (land is Heightmap.Biome.None) continue;
                
                List<Environments> weathers = new();
                switch (land)
                {
                    case Heightmap.Biome.Meadows:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_Meadows_Weather1.Value);
                                weathers.Add(_Winter_Meadows_Weather2.Value);
                                weathers.Add(_Winter_Meadows_Weather3.Value);
                                weathers.Add(_Winter_Meadows_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_Meadows_Weather1.Value);
                                weathers.Add(_Fall_Meadows_Weather2.Value);
                                weathers.Add(_Fall_Meadows_Weather3.Value);
                                weathers.Add(_Fall_Meadows_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_Meadows_Weather1.Value);
                                weathers.Add(_Spring_Meadows_Weather2.Value);
                                weathers.Add(_Spring_Meadows_Weather3.Value);
                                weathers.Add(_Spring_Meadows_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_Meadows_Weather1.Value);
                                weathers.Add(_Summer_Meadows_Weather2.Value);
                                weathers.Add(_Summer_Meadows_Weather3.Value);
                                weathers.Add(_Summer_Meadows_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.BlackForest:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_BlackForest_Weather1.Value);
                                weathers.Add(_Winter_BlackForest_Weather2.Value);
                                weathers.Add(_Winter_BlackForest_Weather3.Value);
                                weathers.Add(_Winter_BlackForest_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_BlackForest_Weather1.Value);
                                weathers.Add(_Fall_BlackForest_Weather2.Value);
                                weathers.Add(_Fall_BlackForest_Weather3.Value);
                                weathers.Add(_Fall_BlackForest_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_BlackForest_Weather1.Value);
                                weathers.Add(_Spring_BlackForest_Weather2.Value);
                                weathers.Add(_Spring_BlackForest_Weather3.Value);
                                weathers.Add(_Spring_BlackForest_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_BlackForest_Weather1.Value);
                                weathers.Add(_Summer_BlackForest_Weather2.Value);
                                weathers.Add(_Summer_BlackForest_Weather3.Value);
                                weathers.Add(_Summer_BlackForest_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.Swamp:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_Swamp_Weather1.Value);
                                weathers.Add(_Winter_Swamp_Weather2.Value);
                                weathers.Add(_Winter_Swamp_Weather3.Value);
                                weathers.Add(_Winter_Swamp_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_Swamp_Weather1.Value);
                                weathers.Add(_Fall_Swamp_Weather2.Value);
                                weathers.Add(_Fall_Swamp_Weather3.Value);
                                weathers.Add(_Fall_Swamp_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_Swamp_Weather1.Value);
                                weathers.Add(_Spring_Swamp_Weather2.Value);
                                weathers.Add(_Spring_Swamp_Weather3.Value);
                                weathers.Add(_Spring_Swamp_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_Swamp_Weather1.Value);
                                weathers.Add(_Summer_Swamp_Weather2.Value);
                                weathers.Add(_Summer_Swamp_Weather3.Value);
                                weathers.Add(_Summer_Swamp_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.Mountain:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_Mountains_Weather1.Value);
                                weathers.Add(_Winter_Mountains_Weather2.Value);
                                weathers.Add(_Winter_Mountains_Weather3.Value);
                                weathers.Add(_Winter_Mountains_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_Mountains_Weather1.Value);
                                weathers.Add(_Fall_Mountains_Weather2.Value);
                                weathers.Add(_Fall_Mountains_Weather3.Value);
                                weathers.Add(_Fall_Mountains_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_Mountains_Weather1.Value);
                                weathers.Add(_Spring_Mountains_Weather2.Value);
                                weathers.Add(_Spring_Mountains_Weather3.Value);
                                weathers.Add(_Spring_Mountains_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_Mountains_Weather1.Value);
                                weathers.Add(_Summer_Mountains_Weather2.Value);
                                weathers.Add(_Summer_Mountains_Weather3.Value);
                                weathers.Add(_Summer_Mountains_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.Plains:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_Plains_Weather1.Value);
                                weathers.Add(_Winter_Plains_Weather2.Value);
                                weathers.Add(_Winter_Plains_Weather3.Value);
                                weathers.Add(_Winter_Plains_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_Plains_Weather1.Value);
                                weathers.Add(_Fall_Plains_Weather2.Value);
                                weathers.Add(_Fall_Plains_Weather3.Value);
                                weathers.Add(_Fall_Plains_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_Plains_Weather1.Value);
                                weathers.Add(_Spring_Plains_Weather2.Value);
                                weathers.Add(_Spring_Plains_Weather3.Value);
                                weathers.Add(_Spring_Plains_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_Plains_Weather1.Value);
                                weathers.Add(_Summer_Plains_Weather2.Value);
                                weathers.Add(_Summer_Plains_Weather3.Value);
                                weathers.Add(_Summer_Plains_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.Mistlands:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_MistLands_Weather1.Value);
                                weathers.Add(_Winter_MistLands_Weather2.Value);
                                weathers.Add(_Winter_MistLands_Weather3.Value);
                                weathers.Add(_Winter_MistLands_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_MistLands_Weather1.Value);
                                weathers.Add(_Fall_MistLands_Weather2.Value);
                                weathers.Add(_Fall_MistLands_Weather3.Value);
                                weathers.Add(_Fall_MistLands_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_MistLands_Weather1.Value);
                                weathers.Add(_Spring_MistLands_Weather2.Value);
                                weathers.Add(_Spring_MistLands_Weather3.Value);
                                weathers.Add(_Spring_MistLands_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_MistLands_Weather1.Value);
                                weathers.Add(_Summer_MistLands_Weather2.Value);
                                weathers.Add(_Summer_MistLands_Weather3.Value);
                                weathers.Add(_Summer_MistLands_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.Ocean:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_Ocean_Weather1.Value);
                                weathers.Add(_Winter_Ocean_Weather2.Value);
                                weathers.Add(_Winter_Ocean_Weather3.Value);
                                weathers.Add(_Winter_Ocean_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_Ocean_Weather1.Value);
                                weathers.Add(_Fall_Ocean_Weather2.Value);
                                weathers.Add(_Fall_Ocean_Weather3.Value);
                                weathers.Add(_Fall_Ocean_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_Ocean_Weather1.Value);
                                weathers.Add(_Spring_Ocean_Weather2.Value);
                                weathers.Add(_Spring_Ocean_Weather3.Value);
                                weathers.Add(_Spring_Ocean_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_Ocean_Weather1.Value);
                                weathers.Add(_Summer_Ocean_Weather2.Value);
                                weathers.Add(_Summer_Ocean_Weather3.Value);
                                weathers.Add(_Summer_Ocean_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.AshLands:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_AshLands_Weather1.Value);
                                weathers.Add(_Winter_AshLands_Weather2.Value);
                                weathers.Add(_Winter_AshLands_Weather3.Value);
                                weathers.Add(_Winter_AshLands_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_AshLands_Weather1.Value);
                                weathers.Add(_Fall_AshLands_Weather2.Value);
                                weathers.Add(_Fall_AshLands_Weather3.Value);
                                weathers.Add(_Fall_AshLands_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_AshLands_Weather1.Value);
                                weathers.Add(_Spring_AshLands_Weather2.Value);
                                weathers.Add(_Spring_AshLands_Weather3.Value);
                                weathers.Add(_Spring_AshLands_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_AshLands_Weather1.Value);
                                weathers.Add(_Summer_AshLands_Weather2.Value);
                                weathers.Add(_Summer_AshLands_Weather3.Value);
                                weathers.Add(_Summer_AshLands_Weather4.Value);
                                break;
                        }
                        break;
                    case Heightmap.Biome.DeepNorth:
                        switch (_Season.Value)
                        {
                            case Season.Winter:
                                weathers.Add(_Winter_DeepNorth_Weather1.Value);
                                weathers.Add(_Winter_DeepNorth_Weather2.Value);
                                weathers.Add(_Winter_DeepNorth_Weather3.Value);
                                weathers.Add(_Winter_DeepNorth_Weather4.Value);
                                break;
                            case Season.Fall:
                                weathers.Add(_Fall_DeepNorth_Weather1.Value);
                                weathers.Add(_Fall_DeepNorth_Weather2.Value);
                                weathers.Add(_Fall_DeepNorth_Weather3.Value);
                                weathers.Add(_Fall_DeepNorth_Weather4.Value);
                                break;
                            case Season.Spring:
                                weathers.Add(_Spring_DeepNorth_Weather1.Value);
                                weathers.Add(_Spring_DeepNorth_Weather2.Value);
                                weathers.Add(_Spring_DeepNorth_Weather3.Value);
                                weathers.Add(_Spring_DeepNorth_Weather4.Value);
                                break;
                            case Season.Summer:
                                weathers.Add(_Summer_DeepNorth_Weather1.Value);
                                weathers.Add(_Summer_DeepNorth_Weather2.Value);
                                weathers.Add(_Summer_DeepNorth_Weather3.Value);
                                weathers.Add(_Summer_DeepNorth_Weather4.Value);
                                break;
                        }
                        break;
                }

                List<EnvEntry> serverEntries = new();
                
                AddToEntries(weathers, serverEntries);
                ServerWeatherMap[land] = serverEntries;
            }
            
            double totalSeconds = (lastEnvironmentChange + _WeatherDuration.Value * 60) - __instance.m_totalSeconds;
            if (_WeatherDuration.Value == 0) totalSeconds = (lastEnvironmentChange + 60) - __instance.m_totalSeconds;

            if (totalSeconds > 3) return;

            lastEnvironmentChange = __instance.m_totalSeconds;

            if (ServerWeatherMap[Heightmap.Biome.Meadows].Count != 0)
            {
                MeadowIndex = (MeadowIndex + 1) % ServerWeatherMap[Heightmap.Biome.Meadows].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.BlackForest].Count != 0)
            {
                BlackForestIndex = (BlackForestIndex + 1) % ServerWeatherMap[Heightmap.Biome.BlackForest].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.Swamp].Count != 0)
            {
                SwampIndex = (SwampIndex + 1) % ServerWeatherMap[Heightmap.Biome.Swamp].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.Mountain].Count != 0)
            {
                MountainIndex = (MountainIndex + 1) % ServerWeatherMap[Heightmap.Biome.Mountain].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.Plains].Count != 0)
            {
                PlainsIndex = (PlainsIndex + 1) % ServerWeatherMap[Heightmap.Biome.Plains].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.Mistlands].Count != 0)
            {
                MistLandsIndex = (MistLandsIndex + 1) % ServerWeatherMap[Heightmap.Biome.Mistlands].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.AshLands].Count != 0)
            {
                AshLandsIndex = (AshLandsIndex + 1) % ServerWeatherMap[Heightmap.Biome.AshLands].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.DeepNorth].Count != 0)
            {
                DeepNorthIndex = (DeepNorthIndex + 1) % ServerWeatherMap[Heightmap.Biome.DeepNorth].Count;
            }

            if (ServerWeatherMap[Heightmap.Biome.Ocean].Count != 0)
            {
                OceanIndex = (OceanIndex + 1) % ServerWeatherMap[Heightmap.Biome.Ocean].Count;
            }

            ServerWeatherIndexes[Heightmap.Biome.Meadows] = MeadowIndex;
            ServerWeatherIndexes[Heightmap.Biome.BlackForest] = BlackForestIndex;
            ServerWeatherIndexes[Heightmap.Biome.Swamp] = SwampIndex;
            ServerWeatherIndexes[Heightmap.Biome.Mountain] = MountainIndex;
            ServerWeatherIndexes[Heightmap.Biome.Plains] = PlainsIndex;
            ServerWeatherIndexes[Heightmap.Biome.Mistlands] = MistLandsIndex;
            ServerWeatherIndexes[Heightmap.Biome.AshLands] = AshLandsIndex;
            ServerWeatherIndexes[Heightmap.Biome.DeepNorth] = DeepNorthIndex;
            ServerWeatherIndexes[Heightmap.Biome.Ocean] = OceanIndex;
            
            UpdateServerWeatherMan();
        }
        private static void ServerSyncedChangeWeather(
            Heightmap.Biome currentBiome, EnvMan __instance, List<EnvEntry> entries, long sec, bool resetTimer = true)
        {
            int serverIndex = GetServerWeatherManIndex(currentBiome);
            __instance.QueueEnvironment(entries[serverIndex].m_environment);
            SetWeatherMan(entries[serverIndex].m_environment);
            currentEnv = entries[serverIndex].m_environment;
            if (resetTimer) lastEnvironmentChange = sec;
            WeatherTweaked = true;
        }
        
        private static int environmentIndex;
        private static double lastEnvironmentChange = EnvMan.instance.m_totalSeconds;
        public static string GetEnvironmentCountDown()
        {
            if (EnvMan.instance == null || !WeatherTweaked || _WeatherTimerEnabled.Value == Toggle.Off) return "";

            double totalSeconds = (lastEnvironmentChange + _WeatherDuration.Value * 60) - EnvMan.instance.m_totalSeconds;
            if (_WeatherDuration.Value == 0) totalSeconds = (lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds;

            int hours = TimeSpan.FromSeconds(totalSeconds).Hours;
            int minutes = TimeSpan.FromSeconds(totalSeconds).Minutes;
            int seconds = TimeSpan.FromSeconds(totalSeconds).Seconds;

            if (totalSeconds < 0) return "";
            
            return hours > 0 ? $"{hours}:{minutes:D2}:{seconds:D2}" : minutes > 0 ? $"{minutes}:{seconds:D2}" : $"{seconds}";
        }
        private static bool LocalWeatherMan(EnvMan __instance, long sec, List<EnvEntry> environments, Heightmap.Biome biome)
        {
            // If client changes biome before timer runs out
            if (lastBiome != biome)
            {
                ChangeWeather(__instance, environments, sec);
                lastBiome = biome;
            }
            
            long duration = _WeatherDuration.Value * 60; // Total seconds

            if (duration == 0)
            {
                if ((lastEnvironmentChange + 60) - EnvMan.instance.m_totalSeconds > 0) return false;
                ChangeWeather(__instance, environments, sec);
            }
            else
            {
                if ((lastEnvironmentChange + _WeatherDuration.Value * 60) - EnvMan.instance.m_totalSeconds > 0) return false;
                ChangeWeather(__instance, environments, sec);
            }

            return false;
        }
        private static void ChangeWeather(EnvMan __instance, List<EnvEntry> environments, long sec)
        {
            environmentIndex = (environmentIndex + 1) % (environments.Count);
            __instance.QueueEnvironment(environments[environmentIndex].m_environment);
            SetWeatherMan(environments[environmentIndex].m_environment);
            currentEnv = environments[environmentIndex].m_environment;
            lastEnvironmentChange = sec;
            WeatherTweaked = true;
        }
    }
    public class EnvironmentEffectData
    {
        public string name = null!;
        public string m_name = null!;
        public Sprite? m_sprite;
        public string? m_start_msg;
        public string? m_tooltip;
        
        public StatusEffect InitEnvEffect()
        {
            ObjectDB obd = ObjectDB.instance;
            obd.m_StatusEffects.RemoveAll(effect => effect is EnvironmentEffect);

            EnvironmentEffect effect = ScriptableObject.CreateInstance<EnvironmentEffect>();
            effect.name = name;
            effect.m_name = m_name;
            effect.m_icon = _WeatherIconEnabled.Value is Toggle.On ? m_sprite : null;
            effect.m_startMessageType = MessageHud.MessageType.TopLeft;
            effect.m_startMessage = m_start_msg;
            effect.m_tooltip = m_tooltip;
            
            obd.m_StatusEffects.Add(effect);

            return effect;
        }
    }
    public class EnvironmentEffect : StatusEffect
    {
        public EnvironmentEffectData data = null!;
        public override string GetIconText() => EnvManPatch.GetEnvironmentCountDown();
        
    }
}