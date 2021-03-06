﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Spider.Engine.Collections;
using Spider.Engine.Core;

namespace Spider.Engine.GamePlay
{
    /// <summary>
    /// A GameAdapter is a base class that can be used to
    /// make it appear as though classes derived from it
    /// are actually part of the Game class itself.
    /// </summary>
    public class GameAdapter : CoreBase, IGame
    {
        protected IGame game;

        public GameAdapter(IGame game)
        {
            this.game = game;
        }

        #region IGameSettings Members

        public Variation Variation { get { return game.Variation; } set { game.Variation = value; } }
        public AlgorithmType AlgorithmType { get { return game.AlgorithmType; } set { game.AlgorithmType = value; } }
        public int Seed { get { return game.Seed; } set { game.Seed = value; } }
        public double[] Coefficients { get { return game.Coefficients; } set { game.Coefficients = value; } }
        public bool Diagnostics { get { return game.Diagnostics; } set { game.Diagnostics = value; } }
        public bool Interactive { get { return game.Interactive; } set { game.Interactive = value; } }
        public int Instance { get { return game.Instance; } set { game.Instance = value; } }

        public bool TraceMoves { get { return game.TraceMoves; } set { game.TraceMoves = value; } }
        public bool TraceStartFinish { get { return game.TraceStartFinish; } set { game.TraceStartFinish = value; } }
        public bool TraceDeals { get { return game.TraceDeals; } set { game.TraceDeals = value; } }
        public bool TraceSearch { get { return game.TraceSearch; } set { game.TraceSearch = value; } }
        public bool ComplexMoves { get { return game.ComplexMoves; } set { game.ComplexMoves = value; } }

        #endregion

        #region IGame Members

        public Pile Shuffled { get { return game.Shuffled; } }
        public Tableau Tableau { get { return game.Tableau; } }
        public IAlgorithm Algorithm { get { return game.Algorithm; } }
        public Tableau FindTableau { get { return game.FindTableau; } }
        public int NumberOfPiles { get { return game.NumberOfPiles; } }
        public int NumberOfSuits { get { return game.NumberOfSuits; } }

        public MoveList Candidates { get { return game.Candidates; } }
        public MoveList SupplementaryList { get { return game.SupplementaryList; } }
        public PileList[] FaceLists { get { return game.FaceLists; } }
        public HoldingStack[] HoldingStacks { get { return game.HoldingStacks; } }
        public RunFinder RunFinder { get { return game.RunFinder; } }

        public void PrepareToFindMoves(Tableau tableau)
        {
            game.PrepareToFindMoves(tableau);
        }

        public void ProcessMove(Move move)
        {
            game.ProcessMove(move);
        }

        public int AddHolding(HoldingSet holdingSet)
        {
            return game.AddHolding(holdingSet);
        }

        public int AddHolding(HoldingSet holdingSet1, HoldingSet holdingSet2)
        {
            return game.AddHolding(holdingSet1, holdingSet2);
        }

        public int AddSupplementary(MoveList supplementaryMoves)
        {
            return game.AddSupplementary(supplementaryMoves);
        }

        public int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            return game.FindHolding(map, holdingStack, inclusive, fromPile, from, fromStart, fromEnd, to, maxExtraSuits);
        }

        public void Print()
        {
            game.Print();
        }

        public static void Print(Game other)
        {
            Game.Print(other);
        }

        public void PrintBeforeAndAfter()
        {
            game.PrintBeforeAndAfter();
        }

        public void PrintMoves()
        {
            game.PrintMoves();
        }

        public void PrintMoves(MoveList moves)
        {
            game.PrintMoves(moves);
        }

        public void PrintCandidates()
        {
            game.PrintCandidates();
        }

        public void PrintViableCandidates()
        {
            game.PrintViableCandidates();
        }

        public void PrintMove(Move move)
        {
            game.PrintMove(move);
        }

        #endregion
    }
}
