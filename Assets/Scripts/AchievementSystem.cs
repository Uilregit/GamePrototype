using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementSystem : Observer
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void OnNotify(object value, Relic.NotificationType notificationType)
    {
        throw new System.NotImplementedException();
    }
}
