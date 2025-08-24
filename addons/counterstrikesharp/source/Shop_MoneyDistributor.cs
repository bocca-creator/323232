using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Utils;
using ShopAPI;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace Shop_MoneyDistributor;

public class Shop_MoneyDistributor : BasePlugin, IPluginConfig<MD_Config>
{
    public override string ModuleName => "[SHOP] Money Distributor";
    public override string ModuleAuthor => "Ganter1234";
    public override string ModuleVersion => "1.3";
    private IShopApi? _api;
    public MD_Config Config { get; set; } = new();
    private Timer? mapTimer = null;
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = IShopApi.Capability.Get();
        if (_api == null) throw new Exception("SHOP CORE NOT LOADED!!!");

        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        if(hotReload) OnMapStart(Server.MapName);

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            AddTimer(0.5f, () => 
            {
                if(Config.RoundStart_Credits != 0)
                {
                    foreach(var player in Utilities.GetPlayers().Where(player => !player.IsHLTV && _api.IsClientAuthorized(player) && player.Team != CsTeam.Spectator))
                    {
                        _api.AddClientCredits(player, Config.RoundStart_Credits);
                        player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer["RoundStart", Config.RoundStart_Credits]));
                    }
                }
            });
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            AddTimer(0.5f, () => 
            {
                if(Config.RoundEnd_Credits != 0)
                {
                    foreach(var player in Utilities.GetPlayers().Where(player => !player.IsHLTV && _api.IsClientAuthorized(player) && player.Team != CsTeam.Spectator))
                    {
                        _api.AddClientCredits(player, Config.RoundEnd_Credits);
                        player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer["RoundEnd", Config.RoundEnd_Credits]));
                    }
                }
            });
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            var player = @event.Attacker;
            var victim = @event.Userid;
            if(player == null || player.IsBot || player == victim) 
                return HookResult.Continue;

            if(Config.PlayerKill_Credits != 0 && player.Team != CsTeam.Spectator)
            {
                _api.AddClientCredits(player, Config.PlayerKill_Credits);
                player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer["PlayerKill", Config.PlayerKill_Credits]));
            }

            if(victim == null || victim.IsBot) 
                return HookResult.Continue;

            if(Config.PlayerDeath_Credits != 0 && victim.Team != CsTeam.Spectator)
            {
                if(Config.PlayerDeath_Credits < 0)
                    _api.TakeClientCredits(victim, Config.PlayerDeath_Credits*-1);
                else
                    _api.AddClientCredits(victim, Config.PlayerDeath_Credits);
                victim.PrintToChat(StringExtensions.ReplaceColorTags(Localizer["PlayerDeath", Config.PlayerDeath_Credits]));
            }
            return HookResult.Continue;
        });
    }

    public void OnMapStart(string mapName)
    {
        if(Config.IntervalGiveCredits != 0.0f)
        {
            if(mapTimer != null)
                mapTimer.Kill();

            mapTimer = AddTimer(Config.IntervalGiveCredits, () => 
            {
                if(_api == null) return;

                foreach(var player in Utilities.GetPlayers().Where(player => !player.IsHLTV && _api.IsClientAuthorized(player) && player.Team != CsTeam.Spectator))
                {
                    _api.AddClientCredits(player, Config.IntervalGiveCreditsCount);
                    player.PrintToChat(StringExtensions.ReplaceColorTags(Localizer["GiveIntervalCredits", Config.IntervalGiveCreditsCount]));
                }
            }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
        }
    }

    public void OnConfigParsed(MD_Config config) { Config = config; }
}
