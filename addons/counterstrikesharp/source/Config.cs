using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Shop_MoneyDistributor;

public class MD_Config : BasePluginConfig
{
    [JsonPropertyName("PlayerKill_Credits")]
    public int PlayerKill_Credits { get; set; } = 2;

    [JsonPropertyName("PlayerDeath_Credits")]
    public int PlayerDeath_Credits { get; set; } = -5;

    [JsonPropertyName("RoundStart_Credits")]
    public int RoundStart_Credits { get; set; } = 5;

    [JsonPropertyName("RoundEnd_Credits")]
    public int RoundEnd_Credits { get; set; } = 5;

    [JsonPropertyName("IntervalGiveCredits")]
    public float IntervalGiveCredits { get; set; } = 60.0f;

    [JsonPropertyName("IntervalGiveCreditsCount")]
    public int IntervalGiveCreditsCount { get; set; } = 5;
}
