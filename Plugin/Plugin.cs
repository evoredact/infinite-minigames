using UnityEngine;

namespace InfiniteMinigames;

public class InfiniteMinigamesPlugin : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.F3)) {
            PluginInfo.Instance.allowInfiniteMinigames = !PluginInfo.Instance.allowInfiniteMinigames;
            Console.WriteLine("Infinite Minigames is now {0}", PluginInfo.Instance.allowInfiniteMinigames);
        }
    }
}
