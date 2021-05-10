using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Observer : MonoBehaviour
{
    public abstract void OnNotify(object value, Relic.NotificationType notificationType, List<Relic> traceList);

    public abstract void OnNotify(int value, StoryRoomSetup.ChallengeType notificationType);
}

public abstract class Subject:MonoBehaviour
{
    private List<Observer> _observers = new List<Observer>();

    public void RegisterObserver(Observer observer)
    {
        _observers.Add(observer);
    }

    public void Notify(object value, Relic.NotificationType notificationType, List<Relic> traceList)
    {
        foreach (var observer in _observers)
            observer.OnNotify(value, notificationType, traceList);
    }
}
