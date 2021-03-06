﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    public class SwapMoveFinder : GameAdapter
    {
        public SwapMoveFinder(Game game)
            : base(game)
        {
            CardMap = new CardMap();
            Used = new FastList<int>();
        }

        private CardMap CardMap { get; set; }
        private FastList<int> Used { get; set; }

        public void Find()
        {
            // Check for swaps.
            int maxExtraSuits = ExtraSuits(FindTableau.NumberOfSpaces);
            for (int from = 0; from < NumberOfPiles; from++)
            {
                Pile fromPile = FindTableau[from];
                int splitRow = fromPile.Count - RunFinder.GetRunUpAnySuit(from);
                int extraSuits = 0;
                HoldingStack holdingStack = HoldingStacks[from];
                for (int fromRow = fromPile.Count - 1; fromRow >= splitRow; fromRow--)
                {
                    if (fromRow < fromPile.Count - 1)
                    {
                        if (fromPile[fromRow].Suit != fromPile[fromRow + 1].Suit)
                        {
                            // This is a cross-suit run.
                            extraSuits++;
                            if (extraSuits > maxExtraSuits + holdingStack.Suits)
                            {
                                break;
                            }
                        }
                    }

                    Check(from, fromRow, extraSuits, maxExtraSuits);
                }
            }
        }

        private void Check(int from, int fromRow, int extraSuits, int maxExtraSuits)
        {
            if (fromRow == 0 && FindTableau.GetDownCount(from) != 0)
            {
                // Would turn over a card.
                return;
            }

            Pile fromPile = FindTableau[from];
            Card fromCard = fromPile[fromRow];
            Card fromCardParent = Card.Empty;
            bool inSequence = true;
            if (fromRow != 0)
            {
                fromCardParent = fromPile[fromRow - 1];
                inSequence = fromCardParent.IsTargetFor(fromCard);
            }
            HoldingStack fromHoldingStack = HoldingStacks[from];
            for (int to = 0; to < NumberOfPiles; to++)
            {
                Pile toPile = FindTableau[to];
                if (to == from || toPile.Count == 0)
                {
                    continue;
                }
                int splitRow = toPile.Count - RunFinder.GetRunUpAnySuit(to);
                int toRow = -1;
                if (inSequence)
                {
                    // Try to find from counterpart in the first to run.
                    toRow = splitRow + (int)(toPile[splitRow].Face - fromCard.Face);
                    if (toRow < splitRow || toRow >= toPile.Count)
                    {
                        // Sequence doesn't contain our counterpart.
                        continue;
                    }
                }
                else
                {
                    // Try to swap with both runs out of sequence.
                    toRow = splitRow;
                    if (fromRow != 0 && !fromCardParent.IsTargetFor(toPile[toRow]))
                    {
                        // Cards don't match.
                        continue;
                    }
                }
                if (toRow == 0)
                {
                    if (fromRow == 0)
                    {
                        // No point in swap both entire piles.
                        continue;
                    }
                    if (FindTableau.GetDownCount(to) != 0)
                    {
                        // Would turn over a card.
                        continue;
                    }
                }
                else if (!toPile[toRow - 1].IsTargetFor(fromCard))
                {
                    // Cards don't match.
                    continue;
                }

                int toSuits = RunFinder.CountSuits(to, toRow);
                if (extraSuits + toSuits <= maxExtraSuits)
                {
                    // Swap with no holding piles.
                    Algorithm.ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow));
                    continue;
                }

                HoldingStack toHoldingStack = HoldingStacks[to];
                if (extraSuits + toSuits > maxExtraSuits + fromHoldingStack.Suits + toHoldingStack.Suits)
                {
                    // Not enough spaces.
                    continue;
                }

                Used.Clear();
                Used.Add(from);
                Used.Add(to);
                int fromHoldingCount = 0;
                int toHoldingCount = 0;
                int fromHoldingSuits = 0;
                int toHoldingSuits = 0;
                while (true)
                {
                    if (fromHoldingCount < fromHoldingStack.Count &&
                        fromHoldingStack[fromHoldingCount].FromRow >= fromRow &&
                        !Used.Contains(fromHoldingStack[fromHoldingCount].To))
                    {
                        Used.Add(fromHoldingStack[fromHoldingCount].To);
                        fromHoldingSuits = fromHoldingStack[fromHoldingCount].Suits;
                        fromHoldingCount++;
                    }
                    else if (toHoldingCount < toHoldingStack.Count &&
                        toHoldingStack[toHoldingCount].FromRow >= toRow &&
                        !Used.Contains(toHoldingStack[toHoldingCount].To))
                    {
                        Used.Add(toHoldingStack[toHoldingCount].To);
                        toHoldingSuits = toHoldingStack[toHoldingCount].Suits;
                        toHoldingCount++;
                    }
                    else
                    {
                        // Out of options.
                        break;
                    }
                    if (extraSuits + toSuits > maxExtraSuits + fromHoldingSuits + toHoldingSuits)
                    {
                        // Not enough spaces.
                        continue;
                    }

                    // We've found a legal swap.
                    Debug.Assert(toRow == 0 || toPile[toRow - 1].IsTargetFor(fromCard));
                    Debug.Assert(fromRow == 0 || fromCardParent.IsTargetFor(toPile[toRow]));
                    HoldingSet fromHoldingSet = new HoldingSet(fromHoldingStack, fromHoldingCount);
                    HoldingSet toHoldingSet = new HoldingSet(toHoldingStack, toHoldingCount);
                    Algorithm.ProcessCandidate(new Move(MoveType.Swap, from, fromRow, to, toRow, AddHolding(fromHoldingSet, toHoldingSet)));
                    break;
                }
            }
        }
    }
}
