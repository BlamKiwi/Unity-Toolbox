/*
 * Copyright (c) 2014, Missing Box Studio
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met: 
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer. 
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution. 
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 * ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MBS;
using UnityEngine;

public sealed class EventManager : MonoBehaviour
{
    #region Public Instance/Unity Variables

    /// <summary>
    ///     Should this event manager be made the global instance?
    /// </summary>
    public bool GlobalInstance = false;

    /// <summary>
    ///     Event Manager performs faster at the cost of more RAM being used if true.
    /// </summary>
    public bool HighPerformance = true;


    /// <summary>
    ///     How long the event manager has in seconds to fire queued events in each frame.
    /// </summary>
    public float MaxTimeWindow = float.PositiveInfinity;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the Global Event Manager instance if one has been set in the Unity Editor, and it has not been destroyed. Null
    ///     otherwise.
    /// </summary>
    public static EventManager Global
    {
        get { return SingletonInstance; }
    }

    #endregion

    #region Private Instance Variables

    /// <summary>
    ///     Holds the singleton instance for the global access queue.
    /// </summary>
    private static EventManager SingletonInstance;

    /// <summary>
    ///     An event queue that is safe for threads to access.
    /// </summary>
    private IQueue ConcurrentEventQueue;

    /// <summary>
    ///     An event queue that is safe for threads to queue actions on the main thread. 
    /// </summary>
    private Queue<Action> ConccurentActionQueue;

    /// <summary>
    ///     The main event queue.
    /// </summary>
    private IQueue EventQueue, BufferQueue;

    /// <summary>
    ///     The backing dictionary that maps event names to listeners and actions.
    /// </summary>
    private IDictionary<string, ICollection<Action<MonoBehaviour, EventArgs>>> Listeners;

    /// <summary>
    ///     Holds which thread is the "Main" thread.
    /// </summary>
    private int MainThreadID;

    #endregion

    #region Public Methods

    /// <summary>
    ///     Adds the listener action for the given event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="action">The listener action.</param>
    public void AddListener(string eventName, Action<MonoBehaviour, EventArgs> action)
    {
        ThrowIfNotMainThread();
        ThrowIfNull(eventName);
        ThrowIfNull(action);
        ThrowIfNotInstanceAction(action);

        // Check that event manager already has event registered 
        if (!Listeners.ContainsKey(eventName))
            // TODO Research viability of using HashSet with Action objects
            // Add backing structure
            Listeners[eventName] = new LinkedList<Action<MonoBehaviour, EventArgs>>();

        // Add the listener
        ICollection<Action<MonoBehaviour, EventArgs>> col = Listeners[eventName];
        if (!col.Contains(action))
            col.Add(action);
    }

    /// <summary>
    ///     Removes the listener action for the given event.
    /// </summary>
    /// <param name="eventName">The event.</param>
    /// <param name="action">The listener action.</param>
    /// <returns>True if successfully removed the listener.</returns>
    public bool RemoveListener(string eventName, Action<MonoBehaviour, EventArgs> action)
    {
        ThrowIfNotMainThread();
        ThrowIfNull(eventName);
        ThrowIfNull(action);

        // Get event listeners for event
        ICollection<Action<MonoBehaviour, EventArgs>> eventLs = Listeners[eventName];

        // If null or empty return false
        if (eventLs == null || eventLs.Count == 0)
            return false;

        // Remove listener
        bool res = eventLs.Remove(action);

        // De-register event if there is no more listeners
        if (eventLs.Count == 0)
            Listeners.Remove(eventName);

        return res;
    }

    /// <summary>
    ///     Triggers the given event on all listeners immediately.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="sender">The sender triggering the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="requiresListeners">Does this event require listeners?</param>
    public void TriggerEvent(string eventName, MonoBehaviour sender, EventArgs args = null,
        bool requiresListeners = false)
    {
        ThrowIfNotMainThread();

        FireEvent(CreateEvent(eventName, sender, args, requiresListeners));
    }

    /// <summary>
    ///     Queues the given event.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="sender">The sender queueing the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="requiresListeners">Does this event require listeners?</param>
    public void QueueEvent(string eventName, MonoBehaviour sender, EventArgs args = null, bool requiresListeners = false)
    {
        ThrowIfNotMainThread();

        EventQueue.Enqueue(CreateEvent(eventName, sender, args, requiresListeners));
    }

    /// <summary>
    ///     Queues an event in a thread safe manner. Update tick blocks concurrent queue.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="sender">The sender queueing the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="requiresListeners">Does this event require listeners?</param>
    public void ThreadSafeQueueEvent(string eventName, MonoBehaviour sender, EventArgs args = null,
        bool requiresListeners = false)
    {
        lock (ConcurrentEventQueue)
            ConcurrentEventQueue.Enqueue(CreateEvent(eventName, sender, args, requiresListeners));
    }

    /// <summary>
    ///     Aborts the given event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="all">Removes all instances if true.</param>
    /// <returns>True if an event was removed.</returns>
    public bool AbortEvent(string eventName, bool all = false)
    {
        ThrowIfNotMainThread();

        return all ? EventQueue.RemoveAll(eventName) : EventQueue.RemoveFirst(eventName);
    }

    /// <summary>
    ///     Clears the event manager of all queued events.
    /// </summary>
    public void ClearAll()
    {
        ThrowIfNotMainThread();

        EventQueue.Clear();
        lock (ConcurrentEventQueue)
            ConcurrentEventQueue.Clear();
    }

    /// <summary>
    ///     Adds the listener to the given event manager in a null safe way.
    /// </summary>
    /// <param name="manager">The event manager.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="action">The listener action.</param>
    /// <returns>True if successful.</returns>
    public static bool SafeAddListener(EventManager manager, string eventName,
        Action<MonoBehaviour, EventArgs> action)
    {
        if (manager == null)
            return false;

        // Add the listener
        manager.AddListener(eventName, action);
        return true;
    }

    /// <summary>
    ///     Removes the listener from the given event manager in a null safe way.
    /// </summary>
    /// <param name="manager">The event manager.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="action">The listener action.</param>
    /// <returns>True if successful.</returns>
    public static bool SafeRemoveListener(EventManager manager, string eventName,
        Action<MonoBehaviour, EventArgs> action)
    {
        // Remove the listener
        return manager != null && manager.RemoveListener(eventName, action);
    }

    /// <summary>
    /// Queues the action to be done on the main thread. 
    /// </summary>
    /// <param name="action">The action to be queued.</param>
    public void QueueOnMainThread(Action action)
    {
        lock (ConccurentActionQueue)
            ConccurentActionQueue.Enqueue(action);
    }
#endregion

    #region Unity Methods

    /// <summary>
    ///     Called when destroyed.
    ///     Manage global instance if needed.
    /// </summary>
    private void OnDestroy()
    {
        if (GlobalInstance)
            SingletonInstance = null;
    }

    /// <summary>
    ///     Called when behaviour is created.
    ///     Set backing data structures.
    /// </summary>
    private void Awake()
    {
        // Get main thread ID
        MainThreadID = Thread.CurrentThread.ManagedThreadId;

        // Setup concurrent event queue
        ConccurentActionQueue = new Queue<Action>();

        // Assert rational values for time
        if (MaxTimeWindow <= 0.0f)
            throw new ArgumentOutOfRangeException("Max Time Window for events should be a positive non zero value: " +
                                                  name);

        // Create backing structures
        if (HighPerformance)
        {
            // Use array based data structures
            EventQueue = new ArrayQueue();
            BufferQueue = new ArrayQueue();
            ConcurrentEventQueue = new ArrayQueue();
            Listeners = new Dictionary<string, ICollection<Action<MonoBehaviour, EventArgs>>>();
        }
        else
        {
            // Use Graph based data structures
            EventQueue = new LinkedListQueue();
            BufferQueue = new LinkedListQueue();
            ConcurrentEventQueue = new LinkedListQueue();
            Listeners = new SortedDictionary<string, ICollection<Action<MonoBehaviour, EventArgs>>>();
        }

        // Log multiple global instances
        if (GlobalInstance && SingletonInstance != null)
        {
            print(SingletonInstance);
            // Ensure we only have 1 global instance
            GlobalInstance = false;

            // Log problem
            print("More than one event manager is set to be the global instance: " + name);
        }
            // Set global instance
        else if (GlobalInstance)
            SingletonInstance = this;
    }

    /// <summary>
    ///     Update called once per frame.
    ///     Fire events until we run out of time this frame.
    /// </summary>
    private void Update()
    {
        // Get the end of the time window
        float maxS = (MaxTimeWindow == float.PositiveInfinity) ? float.PositiveInfinity : Time.time + MaxTimeWindow;

        // Merge threaded event queue with main thread queue
        // TODO Look at using a Concurrent FIFO
        lock (ConcurrentEventQueue)
            while (!ConcurrentEventQueue.IsEmpty())
            {
                // Queue event
                EventQueue.Enqueue(ConcurrentEventQueue.Dequeue());

                // Check for manager abuse
                if (Time.time >= maxS)
                {
                    print("A thread is abusing the event manager: " + name);
                    return;
                }
            }

        // Buffer any events during update tick
        IQueue mainQueue = EventQueue;
        EventQueue = BufferQueue;

        // Process the queue while we still have time and items to process
        while (!mainQueue.IsEmpty())
        {
            // Dequeue event
            EventTuple e = mainQueue.Dequeue();
            FireEvent(e);

            // Check if we still have time
            if (Time.time >= maxS)
            {
                //print("Event manager ran out of time: " + name);
                break;
            }
        }

        // Process the concurrent action queue
        lock (ConccurentActionQueue)
            while (ConccurentActionQueue.Any())
            {
                //print("Firing event");

                // Do action
                ConccurentActionQueue.Dequeue()();

                // Check if we still have time
                if (Time.time >= maxS)
                {
                    //print("Event manager ran out of time: " + name);
                    break;
                }
            }

        // Merge buffered events back into the main queue
        while(!EventQueue.IsEmpty())
            mainQueue.Enqueue(EventQueue.Dequeue());
        EventQueue = mainQueue;
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///     Throws an exception if the given object is null.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    private static void ThrowIfNull(object obj)
    {
        if (obj == null)
            throw new ArgumentNullException();
    }

    /// <summary>
    ///     Fires the given event on all listeners.
    /// </summary>
    /// <param name="e">The event to fire.</param>
    private void FireEvent(EventTuple e)
    {
        ThrowIfNull(e);

        // Check that the event is still registered 
        if (Listeners.ContainsKey(e.Event))
        {
            // Get event listeners
            ICollection<Action<MonoBehaviour, EventArgs>> eventLs = Listeners[e.Event];

            foreach (var a in eventLs)
            {
                a(e.Sender, e.Args);
            }
        }
        // Throw exception if we required listeners
        else if (e.RequiresListeners)
            throw new ExpectedListenersException { Event = e, Manager = this };
    }

    /// <summary>
    ///     Throws an exception if the current thread is not the main thread.
    /// </summary>
    private void ThrowIfNotMainThread()
    {
        if (MainThreadID != Thread.CurrentThread.ManagedThreadId)
            throw new ThreadStateException("Calling thread is not main thread.");
    }

    /// <summary>
    ///     Throw an exception if given action is not instance action.
    /// </summary>
    /// <param name="a">The Action to test.</param>
    private void ThrowIfNotInstanceAction(Action<MonoBehaviour, EventArgs> a)
    {
        if (a.Target == null)
            throw new NotInstanceActionException();
    }

    /// <summary>
    ///     Create an event from the given event information.
    /// </summary>
    /// <param name="eventName">The event name.</param>
    /// <param name="sender">The sender trigerring the event.</param>
    /// <param name="args">The event arguments.</param>
    /// <param name="requiresListeners">Does this event require listeners?</param>
    /// <returns>The new event tuple.</returns>
    private EventTuple CreateEvent(string eventName, MonoBehaviour sender, EventArgs args, bool requiresListeners)
    {
        ThrowIfNull(eventName);
        ThrowIfNull(sender);

        return new EventTuple(eventName, sender, EnsureNonNullArgs(args), requiresListeners);
    }

    /// <summary>
    ///     Ensures that the given event args are not null.
    /// </summary>
    /// <param name="args">The args to check.</param>
    /// <returns>args if not null. EventArgs.Empty if null.</returns>
    private EventArgs EnsureNonNullArgs(EventArgs args)
    {
        if (args == null)
            return EventArgs.Empty;
        return args;
    }

    #endregion
}