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
using System.Linq;

namespace MBS
{
    /// <summary>
    ///     Generates general permutations (repetitions are allowed) of any positive size.
    /// </summary>
    /// <typeparam name="T">The type of items to be permuted.</typeparam>
    public class GeneralPermutationsGenerator<T> : CombinatorialGenerator<T>
    {
        // --- Constructors ---

        /// <summary>
        ///     Constructs a general permutations generator with the given items and output size.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        /// <param name="outputSize">The output list size.</param>
        public GeneralPermutationsGenerator(IEnumerable<T> items, int outputSize) : base(items, outputSize)
        {
        }

        /// <summary>
        ///     Constructs a general permutations generator with the given items. Output size will be the amount of items.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        public GeneralPermutationsGenerator(IEnumerable<T> items) : this(items, items.Count())
        {
        }

        // --- Public Methods ---

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>The Enumerator.</returns>
        public override IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            // If empty input/output list size output empty list
            if (i_DataList.Count == 0 || OutputSize == 0)
                yield return new T[OutputSize];
            // Else start outputting permutations
            else
            {
                // Setup index array
                var indexes = new int[OutputSize];

                // Iterate through each possible permutation
                do
                {
					// Create a new list item to return
					var listItem = new T[OutputSize];
					
					// Map indexes to actual data values
					for (int i = 0; i < OutputSize; i++)
                        listItem[i] = i_DataList[indexes[i]];
						
					// Return the new combination
                    yield return listItem;
				
                    // Update indexes
                    IncrementIndexes(indexes);
                } while (!indexes.All(x => x == 0));
            }
        }

        // --- Protected Methods ---

        /// <summary>
        ///     Computes the Count property value.
        /// </summary>
        /// <returns>The count.</returns>
        protected override ulong ComputeCount()
        {
            // # Permutations with repetition = # elements in list ^ output size
            ulong size = i_DataList.Count == 0 ? 1 : (ulong)i_DataList.Count;
            var res = size;
            for (int i = 1; i < OutputSize; i++)
            {
                ulong newRes = res * size;
                // Check for overflow
                if (newRes < res)
                    throw new OverflowException("An overflow occurred while computing count.");
                res = newRes;
            }
            return res;
        }

        // --- Private Methods ---

        /// <summary>
        ///     Increments the given indexes and carries over addition.
        /// </summary>
        /// <param name="indexes">The indexes.</param>
        private void IncrementIndexes(int[] indexes)
        {
			// Increment the indexes with carry
            for (int i = 0; i < indexes.Length; i++)
            {
                // Increment the current index with wraparound
                indexes[i] = (indexes[i] + 1)%i_DataList.Count;

                // Check if we need to carry over any values
                if (indexes[i] != 0)
					// No more values to carry
                    return;
            }
        }
    }
}
