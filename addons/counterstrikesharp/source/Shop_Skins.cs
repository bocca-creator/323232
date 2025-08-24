using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Newtonsoft.Json.Linq;
using ShopAPI;

namespace Shop_Tags;

public class Shop_Tags : BasePlugin
{
    public override string ModuleName => "[SHOP] Skins";
    public override string ModuleAuthor => "Ganter1234";
    public override string ModuleVersion => "1.4";
    private IShopApi? _api;
	private readonly string CategoryName = "Skins";
    public static JObject? JsonSkins { get; set; }
    public string[,] playerModel = new string[65, 4];
    public override void OnAllPluginsLoaded(bool hotReload)
    {
        _api = IShopApi.Capability.Get();
        if (_api == null) throw new Exception("WHERE SHOP CORE???");

		string Fpath = Path.Combine(ModuleDirectory,"../../configs/plugins/Shop/skins.json");
		if (!File.Exists(Fpath))
			throw new Exception("WHERE CONFIG??? (configs/plugins/Shop/skins.json)");

		JsonSkins = JObject.Parse(File.ReadAllText(Fpath));

		RegisterListener<Listeners.OnMapStart>(OnMapStart);

		RegisterListener<Listeners.OnServerPrecacheResources>((manifest) =>
        {
			foreach (var key in JsonSkins!.Properties())
			{
				if(JsonSkins.TryGetValue(key.Name, out var obj) && obj is JObject JsonItem)
				{
					if(JsonItem["ModelT"]!.ToString().Length > 0)
					{
						Console.WriteLine($"Precaching {(string)JsonItem["ModelT"]!}");
						if(JsonItem["ModelT"]!.ToString().Contains(".vmdl", StringComparison.Ordinal))
							manifest.AddResource((string)JsonItem["ModelT"]!);
						else
							Console.WriteLine("The 'ModelT' parameter must contain the path to the model with the .vmdl extension");
					}
					if(JsonItem["ModelCT"]!.ToString().Length > 0)
					{
						Console.WriteLine($"Precaching {(string)JsonItem["ModelCT"]!}");
						if(JsonItem["ModelCT"]!.ToString().Contains(".vmdl", StringComparison.Ordinal))
							manifest.AddResource((string)JsonItem["ModelCT"]!);
						else
							Console.WriteLine("The 'ModelCT' parameter must contain the path to the model with the .vmdl extension");
					}
				}
			}
        });

        _api.CreateCategory(CategoryName, "Скины");

		foreach (var key in JsonSkins!.Properties())
		{
			if(JsonSkins.TryGetValue(key.Name, out var obj) && obj is JObject JsonItem)
			{
				Task.Run(async () =>
				{
					int ItemID = await _api.AddItem(key.Name, (string)JsonItem["name"]!, CategoryName, (int)JsonItem["price"]!, (int)JsonItem["sellprice"]!, (int)JsonItem["duration"]!);
					_api.SetItemCallbacks(ItemID, OnClientBuyItem, OnClientSellItem, OnClientToggleItem);
				});
			}
		}

		RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
			if(@event == null) return HookResult.Continue;
            var player = @event.Userid;

			if (player == null || !player.IsValid) return HookResult.Continue;

			AddTimer(JsonSkins["SpawnDelay"] == null ? 0.1f : (float)JsonSkins["SpawnDelay"]!, () =>
			{
				if (!player.IsValid
					|| player.PlayerPawn == null
					|| !player.PlayerPawn.IsValid
					|| player.PlayerPawn.Value == null
					|| !player.PlayerPawn.Value.IsValid
					|| player.TeamNum < 2)
					return;

				var playerPawn = player.PlayerPawn?.Value;

				if (playerPawn == null || !playerPawn.IsValid || playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
					return;

				if(!string.IsNullOrWhiteSpace(playerModel[player.Slot, player.TeamNum]))
				{
					SetPlayerModel(playerPawn, playerModel[player.Slot, player.TeamNum]);
				}
			});

            return HookResult.Continue;
        });

		RegisterListener<Listeners.OnClientDisconnect>((playerSlot) =>
		{
			playerModel[playerSlot, 2] = string.Empty;
			playerModel[playerSlot, 3] = string.Empty;
		});
    }

	public void OnClientBuyItem(CCSPlayerController player, int ItemID, string CategoryName, string UniqueName, int BuyPrice, int SellPrice, int Duration, int Count)
	{
		var slot = player.Slot;
		if(JsonSkins!.TryGetValue(UniqueName, out var obj) && obj is JObject JsonItem)
		{
			var playerPawn = player.PlayerPawn?.Value;

			if(JsonItem["ModelT"]!.ToString().Length > 0)
			{
				playerModel[slot, 2] = (string)JsonItem["ModelT"]!;
				if (player.IsValid && playerPawn != null && !playerPawn.IsValid && player.TeamNum == 2)
					SetPlayerModel(playerPawn, playerModel[slot, 2]);
			}
			if(JsonItem["ModelCT"]!.ToString().Length > 0)
			{
				playerModel[slot, 3] = (string)JsonItem["ModelCT"]!;
				if (player.IsValid && playerPawn != null && !playerPawn.IsValid && player.TeamNum == 3)
					SetPlayerModel(playerPawn, playerModel[slot, 3]);
			}
		}
	}
	public void OnClientToggleItem(CCSPlayerController player, int ItemID, string UniqueName, int State)
	{
		var slot = player.Slot;
		if(State == 1)
		{
			if(JsonSkins!.TryGetValue(UniqueName, out var obj) && obj is JObject JsonItem)
			{
				var playerPawn = player.PlayerPawn?.Value;

				if(JsonItem["ModelT"]!.ToString().Length > 0)
				{
					playerModel[slot, 2] = (string)JsonItem["ModelT"]!;
					if (player.IsValid && playerPawn != null && !playerPawn.IsValid && player.TeamNum == 2)
						SetPlayerModel(playerPawn, playerModel[slot, 2]);
				}
				if(JsonItem["ModelCT"]!.ToString().Length > 0)
				{
					playerModel[slot, 3] = (string)JsonItem["ModelCT"]!;
					if (player.IsValid && playerPawn != null && !playerPawn.IsValid && player.TeamNum == 3)
						SetPlayerModel(playerPawn, playerModel[slot, 3]);
				}
			}
		}
		else
		{
			playerModel[slot, 2] = string.Empty;
			playerModel[slot, 3] = string.Empty;

			var playerPawn = player.PlayerPawn?.Value;
			if (playerPawn == null || !playerPawn.IsValid || player.TeamNum < 2)
				return;

			SetDefaultPlayerModel(playerPawn, player.TeamNum);
		}
	}
	public void OnClientSellItem(CCSPlayerController player, int ItemID, string UniqueName, int SellPrice)
	{
		OnClientToggleItem(player, ItemID, UniqueName, 0);
	}
	public void SetDefaultPlayerModel(CCSPlayerPawn pawn, int team)
    {
		string model = team == 2 ? "characters\\models\\tm_phoenix\\tm_phoenix.vmdl" : "characters\\models\\ctm_sas\\ctm_sas.vmdl";
        SetPlayerModel(pawn, model);
    }
	public void SetPlayerModel(CCSPlayerPawn pawn, string model)
    {
        Server.NextFrame(() =>
        {
            pawn.SetModel(model);
        });
    }
	public void OnMapStart(string mapName)
	{
		Server.PrecacheModel("characters\\models\\tm_phoenix\\tm_phoenix.vmdl");
		Server.PrecacheModel("characters\\models\\ctm_sas\\ctm_sas.vmdl");
	}
}
