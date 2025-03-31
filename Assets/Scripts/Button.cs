using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public GameManager game;

    public void OnClick()
    {
        game.SwitchScenes();
    }
}
