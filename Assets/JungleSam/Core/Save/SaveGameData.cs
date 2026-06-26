using System;
using System.Collections.Generic;

[Serializable]
public class SaveGameData
{
    public string userId;
    public string sceneName;
    public string checkpointId;
    public string missionStage;
    public string currentObjective;
    public string secondaryObjective;
    public bool hasPlayerTransform;
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    public float playerRotationY;
    public int health;
    public int armor;
    public string activeWeaponId;
    public int ammo762;
    public int ammo9mm;
    public List<string> startedEncounters = new List<string>();
    public List<string> completedEncounters = new List<string>();
    public List<string> collectedStoryPickups = new List<string>();
    public string savedAtUtc;
}
