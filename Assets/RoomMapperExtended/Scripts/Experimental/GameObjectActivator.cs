using UnityEngine;

public class GameObjectActivator : MonoBehaviour {

    [SerializeField] private GameObject _gameObjectToFace;
    [SerializeField] private GameObject _gameObjectToActivate;
    [SerializeField] private ActivationMethod _activationMethod = ActivationMethod.facingGO;

    private bool IsFacingGameObject() {
        float dot = Vector3.Dot(_gameObjectToActivate.transform.up, _gameObjectToFace.transform.forward);
        return dot < 0;
    }

    private void Update() {
        if (_activationMethod == ActivationMethod.facingGO) {
            bool isCurrentlyFacing = IsFacingGameObject();
            if (isCurrentlyFacing != _gameObjectToActivate.activeSelf) {
                _gameObjectToActivate.SetActive(isCurrentlyFacing);
            }
        }
    }
}