using UnityEngine;
using System;
public class ListenDisable : MonoBehaviour{
    private Action action = null;
    public void WhenDisable(Action action) {
        this.action = action;
    }
    public void OnDisable() {
        if(action != null) {
            action();
        }
    }
}
