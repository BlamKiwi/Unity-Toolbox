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
using UnityEngine;

/// <summary>
/// An event tuple to hold event information.
/// </summary>
public class EventTuple
{
    /// <summary>
    /// The event name.
    /// </summary>
    public string Event { set; get; }
    /// <summary>
    /// The event sender.
    /// </summary>
    public MonoBehaviour Sender;
    /// <summary>
    /// The event arguments.
    /// </summary>
    public EventArgs Args { set; get; }
    /// <summary>
    /// Does this event require listeners?
    /// </summary>
    public bool RequiresListeners { set; get; }

    /// <summary>
    /// Designated event tuple constructor.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The event argument.</param>
    /// <param name="requiresListeners">Does the event require listeners.</param>
    public EventTuple(string eventName, MonoBehaviour sender, EventArgs eventArgs, bool requiresListeners)
    {
        Event = eventName;
        Sender = sender;
        Args = eventArgs;
        RequiresListeners = requiresListeners;
    }

    /// <summary>
    /// Hide the default constructor.
    /// </summary>
    private EventTuple()
    {

    }
}
