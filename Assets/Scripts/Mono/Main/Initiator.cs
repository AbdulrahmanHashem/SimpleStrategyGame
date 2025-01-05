using UnityEngine;

public class Initiator : MonoBehaviour
{
    public GameObject settings;
    public Assets gameAssets;
    public PlayerInput playerInput;
    public GameObject playerCameraController;
    public UnitSelectionManager unitSelectionManager;
    public GameObject gameUICanvas;
    public GameObject environment;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InstantiateGame();
    }

    private void InstantiateGame()
    {
        GameObject settingsInstance = Instantiate(settings);
        Assets gameAssetsInstance = Instantiate(gameAssets);
        PlayerInput playerInputInstance = Instantiate(playerInput);
        GameObject playerCameraControllerGameObjectInstance = Instantiate(playerCameraController);
        UnitSelectionManager unitSelectionManagerInstance = Instantiate(unitSelectionManager);
        GameObject gameUICanvasInstance = Instantiate(gameUICanvas);
        GameObject environmentInstance = Instantiate(environment);
        
        PlayerCameraController playerCameraControllerInstance = 
            playerCameraControllerGameObjectInstance.GetComponentInChildren<PlayerCameraController>();
        
        unitSelectionManagerInstance.playerInput = playerInputInstance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
