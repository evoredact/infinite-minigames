using UnityEngine;

namespace TestPlugin;

public class TestPlugin : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.F3)) {
            PluginInfo.Instance.allowMiniGameStopping = !PluginInfo.Instance.allowMiniGameStopping;
            Console.WriteLine("Minigames Stopping is now {0}", PluginInfo.Instance.allowMiniGameStopping);
        }
    }
}
