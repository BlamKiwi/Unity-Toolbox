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
    ///     Generates general combinations (repetitions are allowed) of any positive size.
    /// </summary>
    /// <typeparam name="T">The type of items to be permuted.</typeparam>
    public class GeneralCombinationsGenerator<T> : CombinatorialGenerator<T>
    {
        // --- Constructors ---

        /// <summary>
        ///     Constructs a general combinations generator with the given items. Output size will be the amount of items.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        public GeneralCombinationsGenerator(IEnumerable<T> items) : base(items)
        {
        }

        /// <summary>
        ///     Constructs a general combinations generator with the given items and output size.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        /// <param name="outputSize">The output list size.</param>
        public GeneralCombinationsGenerator(IEnumerable<T> items, int outputSize) : base(items, outputSize)
        {
        }

        // --- Public Methods --- 

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>The Enumerator.</returns>
        public override IEnumerator<IEnumerable<T>> GetEnumerator()
        {
            // If empty input/output list
            if (OutputSize == 0 || i_DataList.Count == 0)
                yield return new T[] {};
            // Else start outputting items
            else
            {
                // Initialize index array
                int[] indexes = new int[OutputSize];

                // Iterate through each possible combination
                do
                {
                    // Create a new item and return it
                    var listItem = new T[OutputSize];
                    for (int i = 0; i < OutputSize; i++)
                        listItem[i] = i_DataList[indexes[i]];
                    yield return listItem;

                    // Update indexes by incrementing right most and carrying addition
                    int nextIndex = OutputSize - 1;
                    for (int i = nextIndex; i >= 0; i--)
                    {
                        // Update the index
                        indexes[i] = (indexes[i] + 1)%i_DataList.Count;

                        // If there are no more carries to take care of 
                        if (indexes[i] != 0)
                        {
                            // Note the index we are at
                            nextIndex = i;
                            break;
                        }
                    } 

                    // Ensure first ordering property
                    for (int i = nextIndex + 1; i < indexes.Length; i++)
                        indexes[i] = indexes[nextIndex];

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
            // Check for empty edge case
            if (i_DataList.Count == 0)
                return 1;

            // # of items = (n + r - 1)! / r!(n - 1)!
            // Calculate r!
            ulong rFact = Factorial((ulong)OutputSize);

            // Calculate (n - 1)!
            ulong n1Fact = Factorial((ulong)i_DataList.Count - 1);

            // Calculate denominator
            ulong den = rFact * n1Fact;

            // Check for overflow 
            if (den < rFact || den < n1Fact)
                throw  new OverflowException("An overflow occurred trying to compute the Count.");

            // Calculate fraction
            return Factorial((ulong)i_DataList.Count + (ulong)OutputSize - 1) /
                        den;
        }
    }
}
