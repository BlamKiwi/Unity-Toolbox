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
    /// A FIFO Queue implemented as an array.
    /// </summary>
    sealed class ArrayQueue : System.Object, IQueue
    {
        // --- Class Variables ---

        /// <summary>
        /// The initial capacity that the Queue should be.
        /// Must be a power of 2.
        /// </summary>
        private const int INITIAL_CAPACITY = 32;

        // --- Instance Variables ---

        /// <summary>
        /// The amount of current items in the queue.
        /// </summary>
        private int i_CurrentItems;

        /// <summary>
        /// The front and back pointers for the queue.
        /// </summary>
        private int i_Front, i_Back;

        /// <summary>
        /// The backing array.
        /// </summary>
        private EventTuple[] i_Items;

        // --- Constructors ---

        public ArrayQueue()
        {
            i_Items = new EventTuple[INITIAL_CAPACITY];
            Clear();
        }

        // --- Public Methods ---

        bool IQueue.IsEmpty()
        {
            return IsEmpty();
        }

        EventTuple IQueue.Dequeue()
        {
            // Get the front item
            var front = Front();

            // Update array pointers
            if (!IsEmpty())
            {
                // Queue has shrunk in size
                i_CurrentItems--;

                // Drop the head of the queue
                i_Front = Increment(i_Front);
            }

            return front;
        }

        EventTuple IQueue.Front()
        {
            return Front();
        }

        void IQueue.Enqueue(EventTuple item)
        {
            ThrowIfNull(item);

            // Check that the backing array is big enough
            if (i_CurrentItems == i_Items.Length)
                GrowArray();

            // Add item to back of array
            i_Back = Increment(i_Back);
            i_Items[i_Back] = item;

            // Queue has grown
            i_CurrentItems++;
        }

        bool IQueue.RemoveFirst(string item)
        {
            ThrowIfNull(item);

            EventTuple[] newQ = new EventTuple[i_Items.Length * 2];
            var res = false; // Assume we do not remove any items

            // Copy items to new array that do not match given item
            bool singleLock = true; // Lock to ensure we only remove first instance
            int hole = 0; // Hole to place items into
            for (int i = 0; i < i_CurrentItems; i++, i_Front = Increment(i_Front))
            {
                // If item to remove, do not copy to new array
                if (singleLock && i_Items[i_Front].Event.Equals(item))
                {
                    // Ensure future occurrences are not removed
                    singleLock = false;

                    // Removed an item
                    res = true;
                }
                // Else copy the item to the new array
                else
                {
                    newQ[hole] = i_Items[i_Front];
                    hole++;
                }
            }

            // Update array pointers
            UpdateArrayPointers(newQ, hole);

            return res;
        }

        bool IQueue.RemoveAll(string item)
        {
            ThrowIfNull(item);

            EventTuple[] newQ = new EventTuple[i_Items.Length * 2];
            var res = false; // Assume we do not remove any items

            // Copy items to new array that do not match given item
            int hole = 0; // Hole to place items into
            for (int i = 0; i < i_CurrentItems; i++, i_Front = Increment(i_Front))
            {
                // If item to remove, do not copy to new array
                if (i_Items[i_Front].Event.Equals(item))
                    // Removed an item
                    res = true;
                // Else copy the item to the new array
                else
                {
                    newQ[hole] = i_Items[i_Front];
                    hole++;
                }
            }

            // Update array pointers
            UpdateArrayPointers(newQ, hole);

            return res;
        }

        void IQueue.Clear()
        {
            Clear();
        }

        // --- Private Methods ---

        /// <summary>
        /// Clears the queue of all items.
        /// </summary>
        private void Clear()
        {
            i_CurrentItems = i_Front = 0;
            i_Back = -1;
        }

        /// <summary>
        /// Throws an exception if the given item is null;
        /// </summary>
        /// <param name="item">The item to test.</param>
        private void ThrowIfNull(object item)
        {
            if (item == null)
                throw new ArgumentNullException();
        }

        /// <summary>
        /// Increments X with wraparound.
        /// </summary>
        /// <param name="x">The index to increment.</param>
        /// <returns>The new index.</returns>
        private int Increment(int x)
        {
            x++;
            x &= ~i_Items.Length;
            return x;
        }

        /// <summary>
        /// Grows the array to double its capacity.
        /// </summary>
        private void GrowArray()
        {
            EventTuple[] newQ = new EventTuple[i_Items.Length * 2];

            // Copy the elements from old queue to new queue in logical order
            for (int i = 0; i < i_CurrentItems; i++, i_Front = Increment(i_Front))
                newQ[i] = i_Items[i];

            // Update pointers
            UpdateArrayPointers(newQ, i_CurrentItems);
        }

        /// <summary>
        /// Gets the front item in the queue.
        /// </summary>
        /// <returns>The front item. Null if empty.</returns>
        private EventTuple Front()
        {
            if (IsEmpty())
                return null;
            else
                return i_Items[i_Front];
        }

        /// <summary>
        /// Is the array Empty?
        /// </summary>
        /// <returns>True if array is empty.</returns>
        private bool IsEmpty()
        {
            return i_CurrentItems == 0;
        }

        /// <summary>
        /// Updates array pointers after making changes to the backing array.
        /// </summary>
        /// <param name="newQ">The new array.</param>
        /// <param name="hole">The current hole index.</param>
        private void UpdateArrayPointers(EventTuple[] newQ, int hole)
        {
            i_Items = newQ;
            i_Front = 0;
            i_CurrentItems = hole;
            i_Back = i_CurrentItems - 1;
        }
    }
}
