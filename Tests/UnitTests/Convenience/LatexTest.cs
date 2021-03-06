﻿using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class LatexTests
    {
        private readonly VariableEntity x = MathS.Var("x");
        [TestMethod]
        public void TestVar()
        {
            Assert.AreEqual(x.Latexise(), "x");
        }
        [TestMethod]
        public void TestFrac()
        {
            Assert.AreEqual((x / 4).Latexise(), @"\frac{x}{4}");
        }
        [TestMethod]
        public void TestSqrt()
        {
            Assert.AreEqual(MathS.Sqrt(x).Latexise(), @"\sqrt{x}");
        }
        [TestMethod]
        public void TestPow()
        {
            Assert.AreEqual(MathS.Pow(x, x).Latexise(), "{x}^{x}");
        }
        [TestMethod]
        public void TestM1()
        {
            Assert.AreEqual(MathS.Num(-1).ToString(), "-1");
        }
        [TestMethod]
        public void TestMi()
        {
            Assert.AreEqual("i", MathS.Sqrt(-1).Simplify().Latexise());
        }
        [TestMethod]
        public void TestMMi()
        {
            Assert.AreEqual("-i", (-MathS.Sqrt(-1)).Simplify().Latexise());
        }
    }
}
