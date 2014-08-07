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

namespace MBS
{
    /// <summary>
    /// Generates all possible sublists.
    /// </summary>
    /// <typeparam name="T">The type of the list item.</typeparam>
    public class SublistGenerator<T> : CombinatorialGenerator<T>
    {
        // --- Constructors ---

        /// <summary>
        ///     Constructs a sublist generator from the given items.
        /// </summary>
        /// <param name="items">The items to generate lists with.</param>
        public SublistGenerator(IEnumerable<T> items) : base(items)
        {
        }

        // --- Public Methods ---

        public override IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            // The set of all sublists is the number for simple combinations of size N..0
            // Return size N combination 
            yield return i_DataList;

            // Return size n-1 .. 1 combinations
            for (int i = i_DataList.Count - 1; i > 0; i--)
                foreach (var s in new SimpleCombinationsGenerator<T>(i_DataList, i))
                    yield return s;

            // Return size 0 combination
            yield return new T[] {};
        }


        // --- Protected Methods ---

        /// <summary>
        ///     Computes the Count property value.
        /// </summary>
        /// <returns>The count.</returns>
        protected override ulong ComputeCount()
        {
            // Sublist count is equivalent to the number of subsets of indexes
            // count = 2^n. Shift by N
            ulong res = 1UL << i_DataList.Count; 

            // Check for overflow
            // If the count is 0, that means the count bit was shifted out of range
            if (res == 0)
                throw new OverflowException("An overflow occured while trying to compute the count.");
            return res;
        }
    }
}
