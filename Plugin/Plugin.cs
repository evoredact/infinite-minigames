using UnityEngine;

namespace InfiniteMinigames;

public class InfiniteMinigamesPlugin : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.F3)) {
            PluginInfo.Instance.allowMiniGameStopping = !PluginInfo.Instance.allowMiniGameStopping;
            Console.WriteLine("Minigames Stopping is now {0}", PluginInfo.Instance.allowMiniGameStopping);
        }
    }
}
