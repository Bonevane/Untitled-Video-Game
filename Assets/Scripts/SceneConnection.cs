using UnityEngine;

[CreateAssetMenu(menuName = "Levels/SceneConnection")] 
public class SceneConnection : ScriptableObject
{
   public static SceneConnection ActiveConnection { get; set; }
}
