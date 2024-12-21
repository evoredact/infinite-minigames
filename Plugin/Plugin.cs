using UnityEngine;

namespace InfiniteMinigames;

public class InfiniteMinigamesPlugin : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.F3)) {
            PluginInfo.Instance.allowInfiniteMinigames = !PluginInfo.Instance.allowInfiniteMinigames;
            Console.WriteLine("Infinite Minigames is now {0}", PluginInfo.Instance.allowInfiniteMinigames);
        }
        if (Input.GetKeyDown(KeyCode.F2)) {
            GameController gameController = GlobalTag.gameController.GetComponent<GameController>();
            if (!gameController.tetrisPlay) {
                gameController.TetrisStart();
                Console.WriteLine("Tetris equipped");
            }
        }
        //if (Input.GetKeyDown(KeyCode.F1)) {
        //    foreach (QuadLinerMain game in FindObjectsOfType<QuadLinerMain>()) {
        //        for (int i = 0; i < game.waves.Length; i++) {
        //            QuadLinerMain_Wave wave = game.waves[i];
        //            Console.WriteLine("[{0}] wave {1}", i, wave.time);
        //            for (int j = 0; j < wave.enemys.Length; j++) {
        //                int enemy = wave.enemys[j];
        //                Console.WriteLine("[{0}] enemy {1}", j, enemy);
        //            }
        //            for (int j = 0; j < wave.enemysShield.Length; j++) {
        //                int enemy = wave.enemysShield[j];
        //                Console.WriteLine("[{0}] enemyShield {1}", j, enemy);
        //            }
        //        }
        //    }
        //}
    }
}
