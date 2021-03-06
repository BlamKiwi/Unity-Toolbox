﻿/*
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
using System.Collections;
using System.Linq;

namespace MBS
{
    /// <summary>
    ///     Implements all base operations Combinatorial generators require.
    /// </summary>
    /// <typeparam name="T">The type of elements in the output lists.</typeparam>
    public abstract class CombinatorialGenerator<T> : System.Object, IEnumerable<IEnumerable<T>>
    {
        // --- Instance Variables ---

        /// <summary>
        ///     The interim data list for the generator.
        /// </summary>
        protected readonly List<T> i_DataList;

        /// <summary>
        ///     The number of elements contained in the structure.
        /// </summary>
        protected ulong? i_Count;

        // --- Constructors ---

        /// <summary>
        ///     Constructs a combinatorial generator with the given items and output size.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        /// <param name="outputSize">The output list size.</param>
        protected CombinatorialGenerator(IEnumerable<T> items, int outputSize)
        {
            // Ensure valid input
            if (items == null)
                throw new ArgumentNullException("items", "Input items cannot be null.");
            if (outputSize < 0)
                throw new ArgumentOutOfRangeException("outputSize", "Outpust size must be positive.");

            // Set instance variables
            OutputSize = outputSize;
            i_DataList = items.ToList();
        } 

        /// <summary>
        ///     Constructs a combinatorial generator with the given items. Output size will be the number of items.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        protected CombinatorialGenerator(IEnumerable<T> items)
            : this(items, items.Count())
        {
        }


        // --- Public Methods ---

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>The Enumerator.</returns>
        public abstract IEnumerator<IEnumerable<T>> GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>The Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // --- Properties ---

        /// <summary>
        ///     Is the list of all values generated by this generator a finite list?
        /// </summary>
        /// <returns>True if finite values. False if infinite values.</returns>
        public virtual bool IsFinite { get { return true; } }

        /// <summary>
        ///     Gets the number of elements contained in the generator between 1 and ulong.MaxValue. 0 if undefined.
        /// </summary>
        public ulong Count
        {
            get
            {
                // Compute count if we need to
                if (!i_Count.HasValue)
                    i_Count = ComputeCount();

                return i_Count.Value;
            }
        }

        /// <summary>
        ///     Gets and Sets the maximum output list size of the generator.
        /// </summary>
        public int OutputSize { private set; get; }

        // --- Protected Methods ---

        /// <summary>
        ///     Computes the Count property value.
        /// </summary>
        /// <returns>The count.</returns>
        protected abstract ulong ComputeCount();


        /// <summary>
        ///     Calculates x!
        /// </summary>
        /// <param name="x">The number to calculate the factorial of.</param>
        /// <returns>The result of x!</returns>
        protected static ulong Factorial(ulong x)
        {
            // Calculate x!
            ulong res = 1; // 0! == 1

            // Aggregate over interim factorial values
            for (; x > 0; x--)
            {
                // Compute partial factorial into temp accumulator
                ulong newRes = res * x;

                // Check for overflow. Overflow is indicated by value wraparound, so the new accumulator will be smaller than the old one. 
                if (newRes < res)
                    throw  new OverflowException("An overflow occurred while trying to calculate x!");

                // Update the aggregate accumulate
                res = newRes;
            }

            return res;
        }
    }
}