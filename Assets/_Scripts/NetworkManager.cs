using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
    // Start is called before the first frame update
    public static NetworkManager instance;

    public GameObject playerPrefab;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Debug.LogWarning("Instance already exists, destroying object");
            Destroy(this);
        }
    }

    private void Start() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Constants.TICKS_PER_SEC;
    #if UNITY_EDITOR
        Debug.LogError("Build the project to start the server");
    #else
        Server.Start(16, 11000);
    #endif
    }
    
    public Player InstantiatePlayer() {
        return Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Player>();
    }
}
