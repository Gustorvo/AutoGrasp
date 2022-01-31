using UnityEngine;

namespace SoftHand
{
    public interface Ispawnable
    {
        float RrespawnTime { get; set; }
        float MaxDistance { get; set; }
        void SetDefaults();
        void Respawn();
        void SelfDestroy();
    }
    public class Demo : MonoBehaviour
    {
        private Ispawnable[] _allInteractables;

        private void Start()
        {
            _allInteractables = GetComponentsInChildren<Ispawnable>();
            UnityEngine.Debug.Log($"Interactables in the scene: {_allInteractables.Length}");
        }
    }
}
