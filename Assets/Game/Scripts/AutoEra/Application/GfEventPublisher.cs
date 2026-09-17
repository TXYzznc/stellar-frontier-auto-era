using System;
using AutoEra.Events;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace AutoEra.Application
{
    /// <summary>
    /// GF bridge for session facts. Takes ownership of every argument: delivered facts are
    /// released by the event pool, undelivered facts are released here. Outside play mode
    /// there is no event component, so facts are returned to the pool untouched.
    /// </summary>
    public sealed class GfEventPublisher : IEventPublisher
    {
        public void Publish(GameEventArgs args)
        {
            if (args == null) return;
            if (!UnityEngine.Application.isPlaying)
            {
                ReferencePool.Release(args);
                return;
            }

            try
            {
                EventComponent eventComponent = GameEntry.GetComponent<EventComponent>();
                if (eventComponent == null)
                {
                    ReferencePool.Release(args);
                    return;
                }

                eventComponent.Fire(this, args);
            }
            catch (Exception error)
            {
                ReferencePool.Release(args);
                Debug.LogException(error);
            }
        }
    }
}