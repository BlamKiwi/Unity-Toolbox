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
    ///     Generates simple permutations (repetitions are not allowed) of any positive size.
    /// </summary>
    /// <typeparam name="T">The type of items to be permuted.</typeparam>
    public class SimplePermutationsGenerator<T> : CombinatorialGenerator<T>
    {
        // --- Constructors ---

        /// <summary>
        ///     Constructs a simple permutations generator with the given items. Output size will be the amount of items.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        public SimplePermutationsGenerator(IEnumerable<T> items) : base(items)
        {
        }

        /// <summary>
        ///     Constructs a simple permutations generator with the given items and output size.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        /// <param name="outputSize">The output list size.</param>
        public SimplePermutationsGenerator(IEnumerable<T> items, int outputSize) : base(items, outputSize)
        {
            // Check for sane values
            if (OutputSize > i_DataList.Count())
                throw new ArgumentOutOfRangeException("outputSize",
                    "Output list size must be smaller or equal to the amount of items.");
        }

        // --- Public Methods ---

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>The Enumerator.</returns>
        public override IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            // Heap's Algo - Robert Sedgewick's Non-Recrusive implementation
            // http://www.cs.princeton.edu/~rs/talks/perms.pdf

            // Create seed index array
            var seed = new int[i_DataList.Count];
            for (int i = 0; i < i_DataList.Count; i++)
                seed[i] = i;

            // Generate all permutations of simple sub-combinations
            foreach (var seeditem in new SimpleCombinationsGenerator<int>(seed, OutputSize))
            {
                // Setup arrays
                int[] indexes = seeditem.ToArray();
                var counters = new int[OutputSize];

                // Output list item
                yield return OutputListItem(indexes);

                // Generate the rest of the items
                for (int i = 1; i < OutputSize;)
                    // Can we generate another permutation
                    if (counters[i] < i)
                    {
                        // Perform index swap
                        SwapPermutationIndexes(i, counters, indexes);

                        // Output list item
                        yield return OutputListItem(indexes);

                        // Increment and reset counters
                        counters[i]++;
                        i = 1;
                    }
                    else
                        // Reset counters
                        counters[i++] = 0;
            }
        }


        // --- Protected Methods ---

        /// <summary>
        ///     Computes the Count property value.
        /// </summary>
        /// <returns>The count.</returns>
        protected override
            ulong ComputeCount
            ()
        {
            // # of items = n! / (n - r)!
            return Factorial((ulong) i_DataList.Count)/
                   Factorial((ulong) i_DataList.Count - (ulong) OutputSize);
        }

        // --- Private Methods ---

        /// <summary>
        ///     Generates a list item.
        /// </summary>
        /// <param name="indexes">The permutations indexes array.</param>
        /// <returns>The permutation list item.</returns>
        private
            IEnumerable<T> OutputListItem
            (int[]
                indexes)
        {
            var item = new T[OutputSize];
            for (int i = 0; i < OutputSize; i++)
                item[i] = i_DataList[indexes[i]];
            return item;
        }

        /// <summary>
        ///     Swaps the permutation indexes to generate next permutation.
        /// </summary>
        /// <param name="i">Current counter.</param>
        /// <param name="counters">Counter array.</param>
        /// <param name="indexes">Permutation index array.</param>
        private static
            void SwapPermutationIndexes
            (int i, int[] counters, int[] indexes)
        {
            // Get ith or first item index
            int j = i%2*counters[i];

            // Swap indexes
            int old = indexes[j];
            indexes[j] = indexes[i];
            indexes[i] = old;
        }
    }
}