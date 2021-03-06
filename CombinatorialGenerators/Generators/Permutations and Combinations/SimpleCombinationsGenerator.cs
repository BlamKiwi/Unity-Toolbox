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
using System.Linq;

namespace MBS
{
    /// <summary>
    ///     Generates simple combinations (repetitions are not allowed) of any positive size.
    /// </summary>
    /// <typeparam name="T">The type of items to be permuted.</typeparam>
    public class SimpleCombinationsGenerator<T> : GeneralPermutationsGenerator<T>
    {
        // --- Constructors ---

        /// <summary>
        ///     Constructs a simple combinations generator with the given items. Output size will be the amount of items.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        public SimpleCombinationsGenerator(IEnumerable<T> items) : base(items)
        {
        }

        /// <summary>
        ///     Constructs a simple combinations generator with the given items and output size.
        /// </summary>
        /// <param name="items">The items to permute.</param>
        /// <param name="outputSize">The output list size.</param>
        public SimpleCombinationsGenerator(IEnumerable<T> items, int outputSize)
            : base(items, outputSize)
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
            // Check for edge cases
            // If output size is same as input size return size N list
            if (OutputSize == i_DataList.Count)
                yield return i_DataList;
                // Else if empty output list size output empty list
            else if (OutputSize == 0)
                yield return new T[] {};
                // Else start outputting combinations
            else
            {
                // Setup skip array
				// The size of our skip array is the delta 
				// between the data list and the desired output size
                int delta = i_DataList.Count - OutputSize;
                var skip = new int[delta];
				
				// Setup the skip array with some initial values
				// The initial values will be set to the the delta range 
				// between the output size and the data list size.
                for (int i = OutputSize, j = 0; i < i_DataList.Count; i++, j++)
                    skip[j] = i;

                // Start generating combinations
                int nextSkip = 0;
                do
                {
                    // Create list items to return
                    var listItem = new T[OutputSize];
					
					// Start mapping indexes to actual data values
					// i - Data List Index
					// s - Skip Array Index
					// hole - The index of the hole we will place items into
                    for (int i = 0, s = 0, hole = 0; i < i_DataList.Count && hole < OutputSize; i++)
                    {
                        // If item is in skip array, skip it and go to next skip array item
                        if (s < skip.Length && skip[s] == i)
							// Increment skip index to skip item
                            s++;
                            // Else place item into hole
                        else
                        {
							// Place item
                            listItem[hole] = i_DataList[i];
							
							// Increment hole to unfilled array index
                            hole++;
                        }
                    }
					
					// Return the list item
                    yield return listItem;

                    // Update skip array
                    // Find the next skip index to update
                    for (nextSkip = 0; nextSkip < delta && skip[nextSkip] == nextSkip; nextSkip++)
                        ;

                    // If next skip array index is in skip array range
                    if(nextSkip != delta)
						// Update and carry skip indexes over
                        for (int j = nextSkip, i = skip[j] - 1; j >= 0; i--, j--)
                            skip[j] = i;
                } while (nextSkip != delta); // While the skip array item index is still inside the skip array
            }
        }

        // --- Protected Methods ---

        /// <summary>
        ///     Computes the Count property value.
        /// </summary>
        /// <returns>The count.</returns>
        protected override ulong ComputeCount()
        {
            // # of items = n! / r!(n - 1)!
            // Calculate r!
            ulong rFact = Factorial((ulong) OutputSize);

            // Calculate (n - 1)!
            ulong nRFact = Factorial((ulong) i_DataList.Count - (ulong) OutputSize);

            // Calculate denominator
            ulong den = rFact*nRFact;

            // Check for overflow 
            if (den < rFact || den < nRFact)
                throw new OverflowException("An overflow occurred while trying to compute the Count.");

            // Calculate fraction
            return Factorial((ulong) i_DataList.Count)/
                        den;
        }
    }
}
