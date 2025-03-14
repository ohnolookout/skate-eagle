using UnityEngine;
public enum RunState { Landing, Standby, Active, Finished, GameOver, Fallen }
public interface ILevelManager
{
    GroundManager GroundManager { get; set; }
    bool HasPlayer { get; }
    bool HasTerrainManager { get ; }

    void Fall();
    void GameOver(IPlayer player);
    void GoToStandby();
    void RestartLevel();
    void StartAttempt(IPlayer player);
}