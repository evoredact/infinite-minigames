using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace InfiniteMinigames;

public static class PluginInfo {
	public const string PLUGIN_GUID = "org.redact.miside.InfiniteMinigames";
	public const string PLUGIN_NAME = "Infinite Minigames";
	public const string PLUGIN_VERSION = "0.0.2";

	public static PluginLoader Instance;
}

// add gui score for every minigame

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class PluginLoader : BasePlugin {
	public ManualLogSource Logger { get; private set; }

	public PluginLoader() {}

    internal static Harmony harmony = new Harmony("Harmonyy");

    public bool allowInfiniteMinigames = true;

    public override void Load() {
		Logger = (this as BasePlugin).Log;
		PluginInfo.Instance = this;

        Harmony.DEBUG = true;

        harmony.PatchAll(typeof(Patch_MinigamesAutomate));

        harmony.PatchAll(typeof(Patch_Shooter_Main));
        harmony.PatchAll(typeof(Patch_Shooter_Enemy));
        harmony.PatchAll(typeof(Patch_Shooter_Player));

        harmony.PatchAll(typeof(Patch_Location19_Game1));
        harmony.PatchAll(typeof(Patch_Location19_Game2));
        //harmony.PatchAll(typeof(Patch_Location19_Game3));
        harmony.PatchAll(typeof(Patch_Location19_Game4));

        harmony.PatchAll(typeof(Patch_Location4TableCardGame));

        //harmony.PatchAll(typeof(Patch_Location3CutCarrot));

        IL2CPPChainloader.AddUnityComponent(typeof(InfiniteMinigamesPlugin));
	}

    internal static string GetGameObjectPath(GameObject obj) {
        string path = "/" + obj.name;
        while (obj.transform.parent != null) {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    internal static class Patch_MinigamesAutomate {
        [HarmonyPatch(typeof(MinigamesAutomate), "Start")]
        [HarmonyPostfix]
        private static void Start(MinigamesAutomate __instance) {
            __instance.destroyInteractiveOnFinish = false;
        }
    }

    internal class TemporaryShooterSettingz : MonoBehaviour {
        public int killed = 0;
    }
    internal static class Patch_Shooter_Enemy {
        internal static int CountEnemies(Il2CppSystem.Collections.Generic.List<Shooter_Main_TimePart> enemies, int type) {
            int count = 0;
            foreach (Shooter_Main_TimePart timePart in enemies) {
                if (timePart.typeEnemy == type)
                    count++;
            }
            return count;
        }

        [HarmonyPatch(typeof(Shooter_Enemy), "Death")]
        [HarmonyPrefix]
        private static void Death(Shooter_Enemy __instance) {
            if (PluginInfo.Instance.allowInfiniteMinigames && __instance.componentMain.indexWave > 1) {

                TemporaryShooterSettingz settings = __instance.componentMain.gameObject.GetComponent<TemporaryShooterSettingz>();
                settings.killed++;

                Shooter_Main_Wave lastWave = __instance.componentMain.waves[__instance.componentMain.indexWave];
                Console.WriteLine("K: {0}, E: {1}", settings.killed, lastWave.enemys.Count);
                if (__instance.componentMain.indexWave > 1 && settings.killed == lastWave.enemys.Count) {
                    settings.killed = 0;
                    Shooter_Main_Wave wave = new();
                    Il2CppSystem.Collections.Generic.List<Shooter_Main_TimePart> enemies = new();
                    for (int i = 0; i < lastWave.enemys.Count + 7; i++) {
                        int type = UnityEngine.Random.RandomRange(0, 4);
                        if (type == 2 && CountEnemies(enemies, 2) > 4)
                            type = 1;
                        enemies.Add(new() { nextTime = UnityEngine.Random.RandomRange(0f, 0.3f), typeEnemy = type }); // Shooter_Enemy.TypeShooterEnemy
                    }
                    wave.caseWave = GameObject.Instantiate(lastWave.caseWave);
                    wave.enemys = enemies;
                    __instance.componentMain.waves[2] = wave;
                    __instance.componentMain.indexWave = 1;
                    __instance.componentMain.NextWave();
                    //return false;
                }
            }
        }
    }
    internal static class Patch_Shooter_Player {
        [HarmonyPatch(typeof(Shooter_Player), "Death")]
        [HarmonyPrefix]
        private static void Death(Shooter_Player __instance) {
            if (PluginInfo.Instance.allowInfiniteMinigames) {
                __instance.scrmain.RestartFull();
            }
        }
    }
    internal static class Patch_Shooter_Main {
        [HarmonyPatch(typeof(Shooter_Main), "Start")]
        [HarmonyPrefix]
        private static void Start(Shooter_Main __instance) {
            if (!ClassInjector.IsTypeRegisteredInIl2Cpp<TemporaryShooterSettingz>())
                ClassInjector.RegisterTypeInIl2Cpp<TemporaryShooterSettingz>();
            __instance.gameObject.AddComponent<TemporaryShooterSettingz>();
        }

        [HarmonyPatch(typeof(Shooter_Main), "Win")]
        [HarmonyPrefix]
        private static bool Win(Shooter_Main __instance) {
            if (PluginInfo.Instance.allowInfiniteMinigames) {
                return false;
            }
            return true;
        }
    }

    internal static class Patch_Location19_Game1 {
        [HarmonyPatch(typeof(Location19_Game1), "Finish")]
        [HarmonyPrefix]
        private static bool Finish(Location19_Game1 __instance) {
            if (PluginInfo.Instance.allowInfiniteMinigames) {
                __instance.PointsRandom();
                return false;
            }
            return true;
        }
    }
    internal static class Patch_Location19_Game2 {
        [HarmonyPatch(typeof(Location19_Game2), "PointCLick")]
        [HarmonyPrefix]
        private static bool PointClick(Location19_Game2 __instance, int _index) {
            if (PluginInfo.Instance.allowInfiniteMinigames && (__instance.indexPointNeed + 2) == __instance.points._size) {
                __instance.indexPointHold = 0;
                __instance.indexPointNeed = 0;
                __instance.Start();
                return false;
            }
            return true;
        }
    }
    // can't restart the game, might require custom shuffle
    //internal static class Patch_Location19_Game3 {
    //    private static int CountRightPlanets(Location19_Game3_Planet planet) {
    //        return planet.figures.Count(f => {
    //            Location19_Game3_Figure figure = f.GetComponent<Location19_Game3_Figure>();
    //            return figure.figure == figure.planet;
    //        });
    //    }
    //    private static bool ShouldResetGame(Location19_Game3_Planet[] planets) {
    //        int maxFigures = planets[0].figures.Length;
    //        Console.WriteLine("MF: {0}", maxFigures);
    //        var rightFiguresCounts = planets.Select(p => CountRightPlanets(p)).ToArray();
    //        Console.WriteLine("RFC 0: {0}, 1: {1}, 2: {2}", rightFiguresCounts[0], rightFiguresCounts[1], rightFiguresCounts[2]);
    //        Console.WriteLine((rightFiguresCounts[0] == maxFigures - 1 && rightFiguresCounts[1] == maxFigures - 1 && rightFiguresCounts[2] == maxFigures) ||
    //           (rightFiguresCounts[0] == maxFigures && rightFiguresCounts[1] == maxFigures - 1 && rightFiguresCounts[2] == maxFigures - 1) ||
    //           (rightFiguresCounts[0] == maxFigures - 1 && rightFiguresCounts[1] == maxFigures - 1 && rightFiguresCounts[2] == maxFigures));
    //        return (rightFiguresCounts[0] == maxFigures-1 && rightFiguresCounts[1] == maxFigures-1 && rightFiguresCounts[2] == maxFigures) ||
    //           (rightFiguresCounts[0] == maxFigures && rightFiguresCounts[1] == maxFigures-1 && rightFiguresCounts[2] == maxFigures-1) ||
    //           (rightFiguresCounts[0] == maxFigures-1 && rightFiguresCounts[1] == maxFigures-1 && rightFiguresCounts[2] == maxFigures);
    //    }

    //    // idk it just dont work
    //    [HarmonyPatch(typeof(Location19_Game3), "TakeFigure")]
    //    [HarmonyPrefix]
    //    private static bool TakeFigure(Location19_Game3 __instance, int _index, int _planet) {
    //        if (PluginInfo.Instance.allowInfiniteMinigames && ShouldResetGame(__instance.planets)) {
    //            __instance.Start();
    //            return false;
    //        }
    //        return true;
    //    }
    //}
    internal static class Patch_Location19_Game4 {
        [HarmonyPatch(typeof(Location19_Game4), "Shot")]
        [HarmonyPrefix]
        private static void Shot(Location19_Game4 __instance) {
            if (PluginInfo.Instance.allowInfiniteMinigames) {
                int aliveShips = __instance.ships.Count(f => {
                    return f.ship != null;
                });
                if (aliveShips == 1) {
                    __instance.RestartWorld();
                }
            }
        }
    }

    // carrot cut at first meet, a little bit broken
    //internal static class Patch_Location3CutCarrot {
    //    private static Vector3[] positions = null;
    //    private static Quaternion[] rotations = null;

    //    [HarmonyPatch(typeof(Location3CutCarrot), "CutStart")]
    //    [HarmonyPrefix]
    //    private static void CutStart(Location3CutCarrot __instance) {
    //        positions = new Vector3[__instance.timesCut.Length];
    //        rotations = new Quaternion[__instance.timesCut.Length];
    //        for (int i = 0; i < __instance.partsCarrot.Length; i++) {
    //            positions[i] = __instance.partsCarrot[i].transform.position;
    //            rotations[i] = __instance.partsCarrot[i].transform.rotation;
    //        }
    //    }

    //    [HarmonyPatch(typeof(Location3CutCarrot), "Cut")]
    //    [HarmonyPrefix]
    //    private static bool Cut(Location3CutCarrot __instance) {
    //        if (PluginInfo.Instance.allowInfiniteMinigames) {
    //            if (__instance.indexTimeCut > 30) {
    //                __instance.indexTimeCut = 0;
    //                __instance.indexCarrotFall = 0;
    //                __instance.animationTime = 0.18f;
    //                //foreach (GameObject go in __instance.partsCarrot) {
    //                //    go.transform.parent = baseParent;
    //                //    //go.GetComponent<Rigidbody>().isKinematic = true;
    //                //}
    //                //__instance.CutStart();

    //                Transform baseParent = GlobalTag.playerLeftItem.transform.Find("Carrot CutPlayer");
    //                for (int i = 0; i < __instance.partsCarrot.Length; i++) {
    //                    __instance.partsCarrot[i].GetComponent<Rigidbody>().isKinematic = true;
    //                    __instance.partsCarrot[i].transform.position = positions[i];
    //                    __instance.partsCarrot[i].transform.rotation = rotations[i];
    //                    __instance.partsCarrot[i].transform.parent = baseParent;
    //                }
    //                return false;
    //            }
    //            Console.WriteLine("tc: {0}", __instance.timesCut);
    //            Console.WriteLine("icf: {0}", __instance.indexCarrotFall);
    //            Console.WriteLine("itc: {0}", __instance.indexTimeCut);
    //            Console.WriteLine("at: {0}", __instance.animationTime);
    //            Console.WriteLine("p: {0}", __instance.play);
    //            Console.WriteLine("ts: {0}", __instance.timeStart);
    //            //__instance.CutStart();
    //            //return false;
    //        }
    //        return true;
    //    }
    //}

    internal static class Patch_Location4TableCardGame {
        //[HarmonyPatch(typeof(Location4TableCardGame), "Update")]
        //[HarmonyPrefix]
        //private static bool Update(Location4TableCardGame __instance) {
        //    if (PluginInfo.Instance.allowInfiniteMinigames)
        //        if (__instance.countSteps > 3)
        //            __instance.countSteps--;

        //    return true;
        //}

        [HarmonyPatch(typeof(Location4TableCardGame), "LookResult")]
        [HarmonyPrefix]
        private static bool LookResult(Location4TableCardGame __instance) {
            //Console.WriteLine("Checking results");
            if (PluginInfo.Instance.allowInfiniteMinigames) {
                if (__instance.countSteps > 3)
                    __instance.countSteps--;
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
}
