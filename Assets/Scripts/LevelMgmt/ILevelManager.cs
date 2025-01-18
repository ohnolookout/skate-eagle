using UnityEngine;
public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen }
public interface ILevelManager
{
    Level CurrentLevel { get; set; }
    GroundManager TerrainManager { get; set; }
    bool HasPlayer { get; }
    bool HasTerrainManager { get ; }

    void Fall();
    void Finish(IPlayer player);
    void GameOver(IPlayer player);
    void GoToStandby();
    void RestartGame();
    void SetLevel(Level level);
    void StartAttempt(IPlayer player);
}