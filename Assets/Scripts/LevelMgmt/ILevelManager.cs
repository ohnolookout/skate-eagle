using UnityEngine;
public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen }
public interface ILevelManager
{
    ICameraOperator CameraOperator{ get; }
    Level CurrentLevel { get; set; }
    Vector3 FinishPoint { get; set; }
    static IPlayer GetPlayer { get; }
    TerrainManager TerrainManager { get; set; }
    bool HasCameraOperator { get; }
    bool HasPlayer { get; }
    bool HasTerrainManager { get ; }

    void BackToMenu();
    void Fall();
    void Finish(IPlayer player);
    void GameOver(IPlayer player);
    void GoToStandby();
    void RestartGame();
    void SetLevel(Level level);
    void StartAttempt(IPlayer player);
}