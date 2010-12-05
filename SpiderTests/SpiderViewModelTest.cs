﻿using Spider.Solitaire.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows.Input;
using Spider.Engine;
using Spider.GamePlay;

namespace Spider.Tests
{
    /// <summary>
    ///This is a test class for SpiderViewModelTest and is intended
    ///to contain all SpiderViewModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SpiderViewModelTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private readonly string sampleData = @"
            @2|AhAh|Ah9hJsQh3s--3h6s2s4s--6sTs6h8s|3s2sAs3s2hKs-KsQsJsTs
            9s8s7s6s5h-Jh-4h8h7h-KhQhJhKsQsJsTs9s8h7s6h5s4s3s2sAsTh9s8s7
            sAh--2h4h-Kh7h-2sAs-KsQsJsTs9s8s7s6s5s|3h5hQs5s9h4s5sAsTh4s@
        ";

        /// <summary>
        /// A test for SpiderViewModel Constructor
        /// </summary>
        [TestMethod()]
        public void SpiderViewModelConstructorTest1()
        {
            var target = new SpiderViewModel();
            Assert.AreEqual(0, target.Tableau.DiscardPiles.Count);
            Assert.AreEqual(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
            foreach (var pile in target.Tableau.Piles)
            {
                Assert.AreEqual(1, pile.Count);
                Assert.IsInstanceOfType(pile[0], typeof(EmptySpaceViewModel));
            }
            Assert.AreEqual(0, target.Tableau.StockPile.Count);
        }

        /// <summary>
        /// A test for SpiderViewModel Constructor
        /// </summary>
        [TestMethod()]
        public void SpiderViewModelConstructorTest2()
        {
            var target = new SpiderViewModel(sampleData);
            Assert.AreEqual(2, target.Tableau.DiscardPiles.Count);
            Assert.AreEqual(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
            var pile = target.Tableau.Piles[0];
            Assert.AreEqual(11, pile.Count);
            Assert.IsInstanceOfType(pile[0], typeof(DownCardViewModel));
            Assert.AreEqual(1, target.Tableau.StockPile.Count);
        }

        /// <summary>
        /// A test for NewCommand
        /// </summary>
        [TestMethod()]
        public void NewCommandTest()
        {
            var target = new SpiderViewModel();
            target.NewCommand.Execute(null);
            Assert.AreEqual(0, target.Tableau.DiscardPiles.Count);
            Assert.AreEqual(target.Variation.NumberOfPiles, target.Tableau.Piles.Count);
            foreach (var pile in target.Tableau.Piles)
            {
                Assert.IsTrue(pile.Count > 1);
                Assert.IsInstanceOfType(pile[0], typeof(DownCardViewModel));
                Assert.IsInstanceOfType(pile[pile.Count - 1], typeof(UpCardViewModel));
            }
            Assert.IsTrue(target.Tableau.StockPile.Count > 0);
        }

        /// <summary>
        /// A test for SetVariationCommand
        /// </summary>
        [TestMethod()]
        public void SetVariationCommandTest()
        {
            var originalVariation = Variation.Spider2;
            var newVariation = Variation.Spiderette4;

            var target = new SpiderViewModel();

            Assert.AreEqual(originalVariation, target.Variation);
            foreach (var variation in target.Variations)
            {
                Assert.AreEqual(originalVariation == variation.Value, variation.IsChecked);
            }
            Assert.AreEqual(originalVariation.NumberOfPiles, target.Game.NumberOfPiles);

            target.SetVariationCommand.Execute(new VariationViewModel(newVariation, false));

            Assert.AreEqual(newVariation, target.Variation);
            foreach (var variation in target.Variations)
            {
                Assert.AreEqual(newVariation == variation.Value, variation.IsChecked);
            }
            Assert.AreEqual(newVariation.NumberOfPiles, target.Game.NumberOfPiles);
        }

        /// <summary>
        ///A test for SetAlgorithmCommand
        ///</summary>
        [TestMethod()]
        public void SetAlgorithmCommandTest()
        {
            var originalAlgorithm = AlgorithmType.Study;
            var newAlgorithm = AlgorithmType.Search;

            var target = new SpiderViewModel();

            Assert.AreEqual(originalAlgorithm, target.AlgorithmType);
            foreach (var variation in target.Algorithms)
            {
                Assert.AreEqual(originalAlgorithm == variation.Value, variation.IsChecked);
            }

            target.SetAlgorithmCommand.Execute(new AlgorithmViewModel(newAlgorithm, false));

            Assert.AreEqual(newAlgorithm, target.AlgorithmType);
            foreach (var variation in target.Algorithms)
            {
                Assert.AreEqual(newAlgorithm == variation.Value, variation.IsChecked);
            }
        }
    }
}