using UnityEngine;

namespace InfiniteMinigames;

public class InfiniteMinigamesPlugin : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.F2)) {
            foreach (Shooter_Main shooter in FindObjectsOfType<Shooter_Main>()) {
                for (int i = 0; i < shooter.waves.Count; i++) {
                    Shooter_Main_Wave wave = shooter.waves[i];
                    Console.WriteLine("wave: {0}", i);
                    for (int j = 0; j < wave.enemys.Count; j++) {
                        Shooter_Main_TimePart timePart = wave.enemys[j];
                        Console.WriteLine("timePart: {0}, {1}", timePart.typeEnemy, timePart.nextTime);
                    }
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F3)) {
            PluginInfo.Instance.allowInfiniteMinigames = !PluginInfo.Instance.allowInfiniteMinigames;
            Console.WriteLine("Infinite Minigames is now {0}", PluginInfo.Instance.allowInfiniteMinigames);
        }
    }
}
