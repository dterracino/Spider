﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;

namespace Spider.Engine.Core
{
    [DebuggerDisplay("NumberOfPiles = {NumberOfPiles}")]
    public class Tableau : CoreBase, IEnumerable<Pile>, IRunFinder, IGetCard
    {
        static Tableau()
        {
            InitializeZobristKeys();
        }

        public Tableau()
        {
            Variation = Variation.Spider4;
            Initialize();
        }

        public Tableau(Tableau other)
            : this()
        {
            Variation = other.Variation;
            Copy(other);
        }

        public Variation Variation { get; set; }
        public int NumberOfPiles { get; private set; }
        public MoveList Moves { get; private set; }
        public int CheckPoint { get; private set; }

        private int numberOfSpaces;
        private Pile[] downPiles;
        private Pile[] upPiles;
        private bool[] spaceFlags;
        private Pile stockPile;
        private Pile[] discardPileStock;
        private FastList<Pile> discardPiles;
        private FastList<int> spaces;
        private Pile scratchPile;

        private void Initialize()
        {
            NumberOfPiles = Variation.NumberOfPiles;
            Moves = new MoveList();
            stockPile = new Pile();
            downPiles = new Pile[NumberOfPiles];
            upPiles = new Pile[NumberOfPiles];
            spaceFlags = new bool[NumberOfPiles];
            int numberOfDiscardPiles = Variation.NumberOfCards / 13;
            discardPileStock = new Pile[numberOfDiscardPiles];
            for (int i = 0; i < numberOfDiscardPiles; i++)
            {
                discardPileStock[i] = new Pile();
            }
            discardPiles = new FastList<Pile>(NumberOfPiles);
            spaces = new FastList<int>(NumberOfPiles);
            for (int row = 0; row < NumberOfPiles; row++)
            {
                downPiles[row] = new Pile();
                upPiles[row] = new Pile();
            }
            scratchPile = new Pile();
            Refresh();
        }

        public int NumberOfSpaces
        {
            get
            {
                Debug.Assert(numberOfSpaces == SlowNumberOfSpaces);
                return numberOfSpaces;
            }
        }

        private int SlowNumberOfSpaces
        {
            get
            {
                int count = 0;
                for (int column = 0; column < NumberOfPiles; column++)
                {
                    count += upPiles[column].Count == 0 ? 1 : 0;
                }
                return count;
            }
        }

        public Pile this[int index]
        {
            get
            {
                return upPiles[index];
            }
        }

        public Pile StockPile
        {
            get
            {
                return stockPile;
            }
        }

        public IList<Pile> DownPiles
        {
            get
            {
                return downPiles;
            }
        }

        public IList<Pile> UpPiles
        {
            get
            {
                return upPiles;
            }
        }

        public IList<Pile> DiscardPiles
        {
            get
            {
                return discardPiles;
            }
        }

        public IList<int> Spaces
        {
            get
            {
                if (spaces.Count == 0 && NumberOfSpaces != 0)
                {
                    for (int column = 0; column < NumberOfPiles; column++)
                    {
                        if (spaceFlags[column])
                        {
                            spaces.Add(column);
                        }
                    }
                }
                Debug.Assert(Utils.CollectionsAreEqual(spaces, SlowSpaces));
                return spaces;
            }
        }

        private IList<int> SlowSpaces
        {
            get
            {
                var spaces = new FastList<int>();
                for (int column = 0; column < NumberOfPiles; column++)
                {
                    if (upPiles[column].Count == 0)
                    {
                        spaces.Add(column);
                    }
                }
                return spaces;
            }
        }

        public void Clear()
        {
            Moves.Clear();
            CheckPoint = 0;
            if (NumberOfPiles != Variation.NumberOfPiles)
            {
                Initialize();
            }
            stockPile.Clear();
            for (int column = 0; column < NumberOfPiles; column++)
            {
                downPiles[column].Clear();
                upPiles[column].Clear();
            }
            discardPiles.Clear();
            spaces.Clear();
        }

        public bool IsSpace(int column)
        {
            return downPiles[column].Count == 0 && upPiles[column].Count == 0;
        }

        public int GetDownCount(int column)
        {
            return downPiles[column].Count;
        }

        public int GetRunDown(int column, int row)
        {
            return upPiles[column].GetRunDown(row);
        }

        public int GetRunDownAnySuit(int column, int row)
        {
            return upPiles[column].GetRunDownAnySuit(row);
        }

        public int GetRunUp(int column, int row)
        {
            return upPiles[column].GetRunUp(row);
        }

        public int GetRunUpAnySuit(int column)
        {
            return upPiles[column].GetRunUpAnySuit();
        }

        public int GetRunUpAnySuit(int column, int row)
        {
            return upPiles[column].GetRunUpAnySuit(row);
        }

        public int CountSuits(int column)
        {
            return upPiles[column].CountSuits(0, -1);
        }

        public int CountSuits(int column, int row)
        {
            return upPiles[column].CountSuits(row, -1);
        }

        public int CountSuits(int column, int startRow, int endRow)
        {
            return upPiles[column].CountSuits(startRow, endRow);
        }

        public void Copy(Tableau other)
        {
            Clear();
            discardPiles.Copy(other.discardPiles);
            for (int column = 0; column < NumberOfPiles; column++)
            {
                upPiles[column].Copy(other.upPiles[column]);
            }
            for (int column = 0; column < NumberOfPiles; column++)
            {
                downPiles[column].Copy(other.downPiles[column]);
            }
            stockPile.Copy(other.stockPile);
            Refresh();
        }

        public void CopyUpPiles(Tableau other)
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                upPiles[column].Copy(other.upPiles[column]);
            }
            Refresh();
        }

        public void BlockDownPiles(Tableau other)
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                if (other.downPiles[column].Count != 0)
                {
                    downPiles[column].Add(Card.Empty);
                }
            }
        }

        public void Refresh()
        {
            int currentNumberOfSpaces = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                bool isEmpty = upPiles[column].Count == 0;
                spaceFlags[column] = isEmpty;
                currentNumberOfSpaces += isEmpty ? 1 : 0;
            }
            numberOfSpaces = currentNumberOfSpaces;
            spaces.Clear();
        }

        public void Adjust()
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                CheckDiscard(column);
                CheckTurnOverCard(column);
            }
        }

        public Move Normalize(Move move)
        {
            if (move.Type == MoveType.Basic)
            {
                if (move.FromRow < 0)
                {
                    move.FromRow += upPiles[move.From].Count;
                }
                if (move.ToRow == -1)
                {
                    move.ToRow = upPiles[move.To].Count;
                }
            }
            else if (move.Type == MoveType.Swap)
            {
                if (move.FromRow < 0)
                {
                    move.FromRow += upPiles[move.From].Count;
                }
                if (move.ToRow < 0)
                {
                    move.ToRow += upPiles[move.To].Count;
                }
                if (move.FromRow == upPiles[move.From].Count)
                {
                    move.Type = MoveType.Basic;
                    int from = move.From;
                    int fromRow = move.FromRow;
                    move.From = move.To;
                    move.FromRow = move.ToRow;
                    move.To = from;
                    move.ToRow = fromRow;
                }
                if (move.ToRow == upPiles[move.To].Count)
                {
                    move.Type = MoveType.Basic;
                }
            }
            return move;
        }

        public bool IsValid(Move move)
        {
            if (move.Type == MoveType.Basic)
            {
                return MoveIsValid(move);
            }
            else if (move.Type == MoveType.Swap)
            {
                return SwapIsValid(move);
            }
            return false;
        }

        private bool MoveIsValid(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            if (fromRow < 0)
            {
                fromRow += fromPile.Count;
            }
            if (fromRow < 0 || fromRow >= fromPile.Count)
            {
                return false;
            }
            if (fromPile[fromRow].IsEmpty)
            {
                return false;
            }
            if (toRow != -1 && toRow != toPile.Count)
            {
                return false;
            }
            int fromSuits = fromPile.CountSuits(fromRow);
            if (fromSuits == -1)
            {
                return false;
            }
            int numberOfSpaces = NumberOfSpaces;
            if (toPile.Count == 0)
            {
                if (numberOfSpaces == 0)
                {
                    return false;
                }
                numberOfSpaces--;
            }
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
            if (fromSuits - 1 > maxExtraSuits)
            {
                return false;
            }
            if (toPile.Count == 0)
            {
                return true;
            }
            if (!fromPile[fromRow].IsSourceFor(toPile[toPile.Count - 1]))
            {
                return false;
            }
            return true;
        }

        private bool SwapIsValid(Move move)
        {
            int from = move.From;
            int fromRow = move.FromRow;
            int to = move.To;
            int toRow = move.ToRow;

            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            if (fromRow < 0)
            {
                fromRow += fromPile.Count;
            }
            if (fromRow < 0 || fromRow >= fromPile.Count)
            {
                return false;
            }
            if (fromPile[fromRow].IsEmpty)
            {
                return false;
            }
            int fromSuits = fromPile.CountSuits(fromRow);
            if (fromSuits == -1)
            {
                return false;
            }
            if (toRow < 0)
            {
                toRow += toPile.Count;
            }
            if (toRow < 0 || toRow >= toPile.Count)
            {
                return false;
            }
            if (toPile[toRow].IsEmpty)
            {
                return false;
            }
            int toSuits = toPile.CountSuits(toRow);
            if (toSuits == -1)
            {
                return false;
            }
            int numberOfSpaces = NumberOfSpaces;
            int maxExtraSuits = ExtraSuits(numberOfSpaces);
            if (fromSuits + toSuits - 1 > maxExtraSuits)
            {
                return false;
            }
            if (toRow != 0 && !fromPile[fromRow].IsSourceFor(toPile[toRow - 1]))
            {
                return false;
            }
            if (fromRow != 0 && !toPile[toRow].IsSourceFor(fromPile[fromRow - 1]))
            {
                return false;
            }
            return true;
        }

        public bool TryMove(Move move)
        {
            if (!IsValid(move))
            {
                return false;
            }
            Move(move);
            return true;
        }

        public void Move(Move move)
        {
            Debug.Assert(IsValid(Normalize(move)));
            UncheckedMove(move);
        }

        public void UncheckedMove(Move move)
        {
            move = Normalize(move);
            if (move.Type == MoveType.Basic)
            {
                DoMove(move.From, move.FromRow, move.To);
            }
            else if (move.Type == MoveType.Swap)
            {
                DoSwap(move.From, move.FromRow, move.To, move.ToRow);
            }
            else
            {
                throw new InvalidMoveException("unsupported move type");
            }

            AddMove(move);
            OnPileChanged(move.From);
            OnPileChanged(move.To);
        }

        private void DoMove(int from, int fromRow, int to)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];

            int fromCount = fromPile.Count - fromRow;
            int toRow = toPile.Count;

            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);
        }

        public void DoSwap(int from, int fromRow, int to, int toRow)
        {
            Pile fromPile = upPiles[from];
            Pile toPile = upPiles[to];
            int fromCount = fromPile.Count - fromRow;
            int toCount = toPile.Count - toRow;

            scratchPile.Clear();
            scratchPile.AddRange(toPile, toRow, toCount);
            toPile.RemoveRange(toRow, toCount);
            toPile.AddRange(fromPile, fromRow, fromCount);
            fromPile.RemoveRange(fromRow, fromCount);
            fromPile.AddRange(scratchPile, 0, toCount);
        }

        public void PrepareLayout(Pile shuffled)
        {
            stockPile.AddRange(shuffled);
            foreach (LayoutPart layoutPart in Variation.LayoutParts)
            {
                for (int i = 0; i < layoutPart.Count; i++)
                {
                    int column = layoutPart.Column + i;
                    downPiles[column].Push(stockPile.Pop());
                }
            }
            Deal();
        }

        public void Deal()
        {
            if (stockPile.Count == 0)
            {
                throw new Exception("no stock left to deal");
            }
            int columns = Math.Min(NumberOfPiles, stockPile.Count);
            DoDeal(columns);
            AddMove(new Move(MoveType.Deal, columns));
            for (int column = 0; column < columns; column++)
            {
                OnPileChanged(column);
            }
        }

        private void DoDeal(int columns)
        {
            for (int column = 0; column < columns; column++)
            {
                if (stockPile.Count == 0)
                {
                    break;
                }
                upPiles[column].Push(stockPile.Pop());
            }
        }

        private void UndoDeal(int columns)
        {
            for (int column = columns - 1; column >= 0; column--)
            {
                stockPile.Push(upPiles[column].Pop());
            }
        }

        private void OnPileChanged(int column)
        {
            CheckDiscard(column);
            CheckTurnOverCard(column);
            CheckSpace(column);
        }

        private void CheckDiscard(int column)
        {
            Pile pile = upPiles[column];
            if (pile.Count < 13)
            {
                return;
            }
            if (pile[pile.Count - 1].Face != Face.Ace)
            {
                return;
            }
            if (pile[pile.Count - 13].Face != Face.King)
            {
                return;
            }

            int runLength = pile.GetRunUp(pile.Count);
            if (runLength != 13)
            {
                return;
            }
            Discard(column);
        }

        private void Discard(int column)
        {
            DoDiscard(column);
            AddMove(new Move(MoveType.Discard, column));
        }

        private void DoDiscard(int column)
        {
            Pile pile = upPiles[column];
            int row = pile.Count - 13;
            Pile sequence = discardPileStock[discardPiles.Count];
            sequence.Clear();
            sequence.AddRange(pile, row, 13);
            pile.RemoveRange(row, 13);
            discardPiles.Add(sequence);
        }

        private void UndoDiscard(int column)
        {
            Pile discardPile = discardPiles.Pop();
            upPiles[column].AddRange(discardPile);
        }

        private void CheckTurnOverCard(int column)
        {
            if (upPiles[column].Count == 0 && downPiles[column].Count != 0)
            {
                TurnOverCard(column);
            }
        }

        private void TurnOverCard(int column)
        {
            DoTurnOverCard(column);
            AddMove(new Move(MoveType.TurnOverCard, column));
        }

        private void DoTurnOverCard(int column)
        {
            Pile upPile = upPiles[column];
            Pile downPile = downPiles[column];
            upPile.Push(downPile.Pop());
        }

        private void UndoTurnOverCard(int column)
        {
            downPiles[column].Push(upPiles[column].Pop());
        }

        private void CheckSpace(int column)
        {
            bool isSpace = upPiles[column].Count == 0;
            if (isSpace != spaceFlags[column])
            {
                numberOfSpaces += (isSpace ? 1 : 0) - (spaceFlags[column] ? 1 : 0);
                spaceFlags[column] = isSpace;
                spaces.Clear();
            }
        }

        private void AddMove(Move move)
        {
            Moves.RemoveRange(CheckPoint, Moves.Count - CheckPoint);
            Moves.Add(move);
            CheckPoint++;
        }

        public void Revert(int checkPoint)
        {
            while (CheckPoint > checkPoint)
            {
                CheckPoint--;
                Undo(CheckPoint);
            }
            while (CheckPoint < checkPoint)
            {
                Redo(CheckPoint);
                CheckPoint++;
            }
        }

        private void Undo(int index)
        {
            Move move = Moves[index];
            switch (move.Type)
            {
                case MoveType.Basic:
                    DoMove(move.To, move.ToRow, move.From);
                    CheckSpace(move.From);
                    CheckSpace(move.To);
                    break;

                case MoveType.Swap:
                    DoSwap(move.From, move.FromRow, move.To, move.ToRow);
                    CheckSpace(move.From);
                    CheckSpace(move.To);
                    break;

                case MoveType.Deal:
                    UndoDeal(move.From);
                    Refresh();
                    break;

                case MoveType.Discard:
                    UndoDiscard(move.From);
                    CheckSpace(move.From);
                    break;

                case MoveType.TurnOverCard:
                    UndoTurnOverCard(move.From);
                    CheckSpace(move.From);
                    break;
            }
        }

        private void Redo(int index)
        {
            Move move = Moves[index];
            switch (move.Type)
            {
                case MoveType.Basic:
                    DoMove(move.From, move.FromRow, move.To);
                    CheckSpace(move.From);
                    CheckSpace(move.To);
                    break;

                case MoveType.Swap:
                    DoSwap(move.From, move.FromRow, move.To, move.ToRow);
                    CheckSpace(move.From);
                    CheckSpace(move.To);
                    break;

                case MoveType.Deal:
                    DoDeal(move.From);
                    Refresh();
                    break;

                case MoveType.Discard:
                    DoDiscard(move.From);
                    CheckSpace(move.From);
                    break;

                case MoveType.TurnOverCard:
                    DoTurnOverCard(move.From);
                    CheckSpace(move.From);
                    break;
            }
        }

        public int GetHashKey()
        {
            int hash = 0;
            int offset = 0;
            for (int row = 0; row < discardPiles.Count; row++)
            {
                hash ^= ZobristKeys[offset][row][discardPiles[row][12].HashKey];
            }
            offset++;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hash ^= GetZobristKey(column + offset, downPiles[column]);
            }
            offset += NumberOfPiles;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hash ^= GetZobristKey(column + offset, upPiles[column]);
            }
            offset += NumberOfPiles;
            hash ^= GetZobristKey(offset, stockPile);
            return hash;
        }

        public int GetUpPilesHashKey()
        {
            int hashKey = 0;
            for (int column = 0; column < NumberOfPiles; column++)
            {
                hashKey ^= GetZobristKey(column, upPiles[column]);
            }
            return hashKey;
        }

        private static int GetZobristKey(int column, Pile pile)
        {
            int hashKey = 0;
            int[][] keys = ZobristKeys[column];
            for (int row = 0; row < pile.Count; row++)
            {
                hashKey ^= keys[row][pile[row].HashKey];
            }
            return hashKey;
        }

        private static int[][][] ZobristKeys;

        private static void InitializeZobristKeys()
        {
            Random random = new Random(0);
            int columns = 2 * 10 + 2;
            int rows = 52 * 2;
            int cards = 13 + 14 * 4 + 1;
            ZobristKeys = new int[columns][][];
            for (int column = 0; column < columns; column++)
            {
                ZobristKeys[column] = new int[rows][];
                for (int row = 0; row < rows; row++)
                {
                    ZobristKeys[column][row] = new int[cards];
                    for (int card = 0; card < cards; card++)
                    {
                        ZobristKeys[column][row][card] = random.Next();
                    }
                }
            }
        }

        #region IEnumerable<Pile> Members

        public IEnumerator<Pile> GetEnumerator()
        {
            for (int column = 0; column < NumberOfPiles; column++)
            {
                yield return upPiles[column];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IGetCard Members

        public Card GetCard(int column)
        {
            return upPiles[column].LastCard;
        }

        #endregion
    }
}
