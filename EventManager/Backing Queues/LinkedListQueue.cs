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

namespace MBS
{
    /// <summary>
    /// A FIFO Queue implemented as a Linked List.
    /// </summary>
    sealed class LinkedListQueue : System.Object, IQueue 
    {
        // --- Instance Variables ---

        /// <summary>
        /// The head and tail of the list.
        /// </summary>
        private ListNode i_Head = null, i_Tail = null;

        // --- Public Methods ---

        bool IQueue.IsEmpty()
        {
            return IsEmpty();
        }

        EventTuple IQueue.Dequeue()
        {
            // Get the front item
            var item = Front();

            // Remove the head of the list
            if (!IsEmpty())
                i_Head = i_Head.m_Next;

            return item;
        }

        EventTuple IQueue.Front()
        {
            return Front();
        }

        void IQueue.Enqueue(EventTuple item)
        {
            ThrowIfNull(item);

            // Create new list node to hold item
            var node = new ListNode() { m_Item = item, m_Next = null };

            // If empty create singleton list
            if (IsEmpty())
                i_Head = i_Tail = node;
            // Else append item to tail
            else
                i_Tail = i_Tail.m_Next = node;
        }

        bool IQueue.RemoveFirst(string item)
        {
            ThrowIfNull(item);

            // Iterate through list and remove first matching item
            ListNode prev = null, current = i_Head;
            while (current != null)
            {
                // Check if current item matches
                if (current.m_Item.Event.Equals(item))
                {
                    // Check for singleton edge case
                    if (i_Head == i_Tail)
                        i_Head = i_Tail = current = null;
                    // Check if we need to delete the tail
                    else if (current == i_Tail)
                    {
                        // Set tail to prev and clear prev's next pointer
                        i_Tail = prev;

                        if (prev != null)
                            prev.m_Next = i_Tail.m_Next;
                    }
                    // Check if we need to move the tail back
                    else if (current.m_Next == i_Tail)
                    {
                        // Copy the tail's contents
                        current.m_Item = current.m_Next.m_Item;
                        current.m_Next = current.m_Next.m_Next;

                        // Move tail back to current node
                        i_Tail = current;
                    }
                    // General case - Copy contents of next node
                    else
                    {
                        current.m_Item = current.m_Next.m_Item;
                        current.m_Next = current.m_Next.m_Next;
                    }

                    // Removed an item
                    return true;
                }
                // Go to next node
                else
                {
                    // Iterate pointers
                    prev = current;
                    current = current.m_Next;
                }
            }

            // Did not remove any items
            return false;
        }

        bool IQueue.RemoveAll(string item)
        {
            ThrowIfNull(item);

            // Assume we do not remove any items
            var res = false;

            // Iterate through list and remove first matching item
            ListNode prev = null, current = i_Head;
            while (current != null)
            {
                // Check if current item matches
                if (current.m_Item.Event.Equals(item))
                {
                    // Check for singleton edge case
                    if (i_Head == i_Tail) 
                        i_Head = i_Tail = current = null;
                    // Check if we need to delete the tail
                    else if (current == i_Tail)
                    {
                        // Set tail to prev and clear prev's next pointer
                        i_Tail = prev;

                        if (prev != null)
                            prev.m_Next = i_Tail.m_Next;
                    }
                    // Check if we need to move the tail back
                    else if (current.m_Next == i_Tail)
                    {
                        // Copy the tail's contents
                        current.m_Item = current.m_Next.m_Item;
                        current.m_Next = current.m_Next.m_Next;

                        // Move tail back to current node
                        i_Tail = current;
                    }
                    // General case - Copy contents of next node
                    else
                    {
                        current.m_Item = current.m_Next.m_Item;
                        current.m_Next = current.m_Next.m_Next;
                    }

                    // Removed an item
                    res = true;
                }
                // Go to next node
                else
                {
                    // Iterate pointers
                    prev = current;
                    current = current.m_Next;
                }
            }

            return res;
        }

        void IQueue.Clear()
        {
            i_Head = i_Tail = null;
        }

        // --- Private Methods ---

        /// <summary>
        /// Is the list empty?
        /// </summary>
        /// <returns>True if the list is empty.</returns>
        private bool IsEmpty()
        {
            return i_Head == null;
        }

        /// <summary>
        /// Returns the front of the list.
        /// </summary>
        /// <returns>The front of the list. Null if empty.</returns>
        private EventTuple Front()
        {
            // Return null if empty
            if (IsEmpty())
                return null;
            // Else return head
            else
                return i_Head.m_Item;
        }

        /// <summary>
        /// Throws an exception if the given item is null.
        /// </summary>
        /// <param name="item">The item to check.</param>
        private static void ThrowIfNull(object item)
        {
            if (item == null)
                throw new ArgumentNullException();
        }

        // --- Private Structures ---

        /// <summary>
        /// Internal class modelling a linked list node.
        /// </summary>
        private class ListNode
        {
            /// <summary>
            /// The item in the list.
            /// </summary>
            public EventTuple m_Item { set; get; }
            /// <summary>
            /// The next list node.
            /// </summary>
            public ListNode m_Next { set; get; }
        }
    }
}
