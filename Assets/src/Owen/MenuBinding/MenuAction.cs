using UnityEngine;
using UnityEngine.EventSystems;

public abstract class MenuAction : MonoBehaviour
{
    public abstract void Execute(); // dynamic binding

    //static binding logging. Uses event system instead of this.name to avoid naming the game object
    public void LogClick()
{
    GameObject clicked = EventSystem.current.currentSelectedGameObject;
    if(clicked != null)
        Debug.Log($"Button clicked: {clicked.name}");
    else
        Debug.Log($"Button clicked: {this.name}");
}

}
