using System;
using UnityEngine;

public class BotEvent : MonoBehaviour
{
    public static Action<GameObject> OnBotDied;

    public static void RaiseBotDied(GameObject bot)
    {
        OnBotDied?.Invoke(bot);
    }
}
