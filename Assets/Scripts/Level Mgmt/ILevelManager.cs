using UnityEngine;

public interface ILevelManager
{
    ICameraOperator CameraOperator{ get; }
    Level CurrentLevel { get; set; }
    Vector3 FinishPoint { get; set; }
    IPlayer GetPlayer { get; }
    TerrainManager TerrainManager { get; set; }
    RunState RunState { get; set; }
    bool HasCameraOperator { get; }
    bool HasPlayer { get; }
    bool HasTerrainManager { get ; }

    void BackToMenu();
    void Fall();
    void Finish();
    void GameOver();
    void GoToStandby();
    void RestartGame();
    void SetLevel(Level level);
    void StartAttempt();
}