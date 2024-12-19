using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Reflection;

namespace InfiniteMinigames;

public static class PluginInfo {
	public const string PLUGIN_GUID = "org.rust.miside.InfiniteMinigames";
	public const string PLUGIN_NAME = "Infinite Minigames";
	public const string PLUGIN_VERSION = "0.0.1";

	public static PluginLoader Instance;
}

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class PluginLoader : BasePlugin {
	public ManualLogSource Logger { get; private set; }

	public PluginLoader() {}

    internal static Harmony harmony = new Harmony("sexyy");

    public bool allowMiniGameStopping = true;

    public override void Load() {
		Logger = (this as BasePlugin).Log;
		PluginInfo.Instance = this;

        Harmony.DEBUG = true;

        harmony.Patch(typeof(Location4TableCardGame).GetMethod("Update"), new HarmonyMethod(typeof(PluginLoader).GetMethod("Prefix_CardUpdate", BindingFlags.Static | BindingFlags.NonPublic)));
        harmony.Patch(typeof(Location4TableCardGame).GetMethod("LookResult"), new HarmonyMethod(typeof(PluginLoader).GetMethod("Prefix_CardLookResult", BindingFlags.Static | BindingFlags.NonPublic)));
        harmony.PatchAll();

        IL2CPPChainloader.AddUnityComponent(typeof(InfiniteMinigamesPlugin));
	}

    private static bool Prefix_CardUpdate(ref Location4TableCardGame __instance) {
        if (!PluginInfo.Instance.allowMiniGameStopping)
            if (__instance.countSteps > 3)
                __instance.countSteps--;

        return true;
    }

    private static bool Prefix_CardLookResult(ref Location4TableCardGame __instance) {
        //Console.WriteLine("Checking results");
        if (!PluginInfo.Instance.allowMiniGameStopping) {
            //Console.WriteLine("IsPlayerHoldingCards: {0}", __instance.playerHoldCards);
            if (__instance.playerHoldCards) {
                int playerCardsCount = -1;
                foreach (Location4TableCardGame_CardHold cardHold in __instance.cardsPlayer) {
                    Location4TableCardGame_CardMemory cardMemory = __instance.cardsGeneral[cardHold.indexMemory];
                    if (cardHold.objectCard && cardHold.objectCard.GetComponent<Location4TableCardGame_Card>().timeDestroy == 0)
                        playerCardsCount++;
                    else continue;
                }
                //Console.WriteLine("Player's cards count: {0}", playerCardsCount);
                if (playerCardsCount == 0) {
                    __instance.playerHoldCards = false;
                    __instance.mitaHoldCards = false;

                    foreach (Location4TableCardGame_CardHold cardHold in __instance.cardsPlayer)
                        if (cardHold.objectCard)
                            cardHold.objectCard.GetComponent<Location4TableCardGame_Card>().timeDestroy = 0.1f;
                    foreach (Location4TableCardGame_CardHold cardHold in __instance.cardsMita)
                        if (cardHold.objectCard)
                            cardHold.objectCard.GetComponent<Location4TableCardGame_Card>().timeDestroy = 0.1f;

                    __instance.MitaTakeCard();
                    __instance.PlayerTakeCard();
                }
            }
        }

        return true;
    }
}
