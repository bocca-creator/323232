using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Newtonsoft.Json;
using CounterStrikeSharp.API.Modules.Admin;

namespace DeadShowImage
{
    public class DeadShowImage : BasePlugin
    {
        public override string ModuleName => "DeadShowImage";
        public override string ModuleVersion => "v1.2.1";
        public override string ModuleAuthor => "CYBERC4T";

        private string currentImage;
        private bool isShowImage;
        private int numberImage = 0;

        private static Config settings;

        FileSystemWatcher watcherConfig;

        public class Config
        {
            public List<string> ImagesPath { get; set; }
            public float DurationImageDisplay { get; set; }
            public float DurationImageNext { get; set; }
            public string HudTextFormat { get; set; }
            public string ImmunityFlag { get; set; }
            public int SelectImageMode { get; set; }
            public bool ShowImageForSpec { get; set; }

            public static Config LoadConfig(string configPath)
            {
                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }

        public override void Load(bool hotReload)
        {
            string configPath = Path.Combine(ModuleDirectory, "settings.json");
            settings = Config.LoadConfig(configPath);

            watcherConfig = new FileSystemWatcher(ModuleDirectory, "settings.json");
            watcherConfig.NotifyFilter = NotifyFilters.LastWrite;
            watcherConfig.Changed += (sender, e) =>
            {
                OnEditConfigFile(sender, e, configPath);
            };
            watcherConfig.EnableRaisingEvents = true;

            AddTimer(settings.DurationImageNext, ShowTimerImage, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);

            RegisterListener<Listeners.OnTick>(() =>
            {
                if (isShowImage)
                {
                    foreach (var player in GetOnlinePlayers())
                    {
                        if (player != null && !player.IsBot && !player.PawnIsAlive && (player.Team == CsTeam.Terrorist || player.Team == CsTeam.CounterTerrorist || (settings.ShowImageForSpec && player.Team == CsTeam.Spectator)))
                        {
                            if (!AdminManager.PlayerHasPermissions(player, settings.ImmunityFlag)) PrintHtml(player, $"{settings.HudTextFormat.Replace("{PATH}", currentImage)}");
                        }
                    }
                }
            });
        }

        private static void OnEditConfigFile(object sender, FileSystemEventArgs e, string configPath)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                settings = Config.LoadConfig(configPath);
            }
        }

        public void ShowTimerImage()
        {
            if (settings.SelectImageMode == 1)
            {
                Random random = new Random();
                int randomImageNum = random.Next(0, settings.ImagesPath.Count);
                currentImage = settings.ImagesPath[randomImageNum];
            }
            else if (settings.SelectImageMode == 2)
            {
                currentImage = settings.ImagesPath[numberImage];

                numberImage++;

                if (numberImage == settings.ImagesPath.Count)
                {
                    numberImage = 0;
                }

            }
            
            isShowImage = true;

            AddTimer(settings.DurationImageDisplay, () =>
            {
                isShowImage = false;
            });
        }

        public void PrintHtml(CCSPlayerController player, string hudContent) // thn for deafps
        {
            var @event = new EventShowSurvivalRespawnStatus(false)
            {
                LocToken = hudContent,
                Duration = 5,
                Userid = player
            };
            @event.FireEvent(false);

            @event = null;
        }

        public static List<CCSPlayerController> GetOnlinePlayers()
        {
            var players = Utilities.GetPlayers();

            List<CCSPlayerController> validPlayers = new List<CCSPlayerController>();

            foreach (var p in players)
            {
                if (p == null) continue;
                if (!p.IsValid) continue;
                if (p.IsBot) continue;
                if (p.Connected != PlayerConnectedState.PlayerConnected) continue;
                validPlayers.Add(p);
            }
            return validPlayers;
        }
    }
}