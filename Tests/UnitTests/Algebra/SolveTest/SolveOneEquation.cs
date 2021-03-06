﻿using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SolveOneEquation
    {
        public static VariableEntity x = "x";

        /// <summary>
        /// Numerically checks if a root fits an equation
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="toSub"></param>
        /// <param name="varValue"></param>
        public void AssertRoots(Entity equation, VariableEntity toSub, Entity varValue)
        {
            string LimitString(string s)
            {
                if (s.Length < 30)
                    return s;
                else
                    return s.Substring(0, 10) + "..." + s.Substring(s.Length - 10, 10);
            }
            string eqNormal = equation.ToString();
            var err = CheckRoots(equation, toSub, varValue);
            Assert.IsTrue(err < 0.001, "Error is : " + err + "  " + LimitString(eqNormal) + "  wrong root is " + toSub.Name + " = " + LimitString(varValue.ToString()));
        }

        public double CheckRoots(Entity equation, VariableEntity toSub, Entity varValue)
        {
            equation = equation.Substitute(toSub, varValue);
            var allVars = MathS.Utils.GetUniqueVariables(equation);
            
            foreach (var vr in allVars.FiniteSet())
                equation = equation.Substitute(vr.Name, 3
                    /* MUST be integer to correspond to integer coefficient of periodic roots*/);

            return Number.Abs(equation.Eval());
        }

        public void AssertRootCount(Set roots, int target)
        {
            Assert.IsFalse(roots.Power == Set.PowerLevel.INFINITE, "Set of roots must be finite");
            Assert.IsTrue(roots.Count == target, string.Format("Number of roots must be equal {0} but is {1}", target, roots.Count));
        }

        [TestMethod]
        public void Test1()
        {
            var eq = (x - 1) * (x - 2);
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void Test2()
        {
            var eq = MathS.Sqr(x) + 1;
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void Test3()
        {
            var eq = new NumberEntity(1);
            var roots = eq.SolveNt(x);
            AssertRootCount(roots, 0);
        }
        [TestMethod]
        public void Test4()
        {
            var eq = x.Pow(2) + 2 * x + 1;
            var tmpEQTHR = MathS.Utils.EQUALITY_THRESHOLD;
            MathS.Utils.EQUALITY_THRESHOLD = 1e-6;
            var roots = eq.SolveNt(x, precision: 100);
            MathS.Utils.EQUALITY_THRESHOLD = tmpEQTHR;
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test5()
        {
            // solve x2 + 2x + 2
            var eq = x.Pow(2) + 2 * x + 2;
            var roots = eq.SolveEquation("x");
            var r1 = MathS.FromString("-1 + 1i").Simplify();
            var r2 = MathS.FromString("-1 - 1i").Simplify();
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
            var s = new Set();
        }

        [TestMethod]
        public void Test6()
        {
            // solve 2x2 + 4x + 2
            var eq = 2 * x.Pow(2) + 4 * x + 2;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test7()
        {
            // solve x2 - 3x + 2
            var eq = x.Pow(2) - 3 * x + 2;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test8()
        {
            // solve x3 + 3x2 + 3x + 1
            var eq = x.Pow(3) + 3 * x.Pow(2) + 3 * x + 1;
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }

        [TestMethod]
        public void Test9()
        {
            // solve x3 - 6x2 + 11x - 6
            var eq = x.Pow(3) - 6 * x.Pow(2) + 11 * x - 6;
            var roots = eq.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestAllNumbers3()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.SolveEquation(x).FiniteSet())
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers3complex()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.SolveEquation(x).FiniteSet())
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers3complexCount()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            int WA = 0;
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.SolveEquation(x).FiniteSet())
                    WA += CheckRoots(newexpr, x, root) > 0.001 ? 1 : 0;
            }
            Assert.IsTrue(WA == 0, "WA count: " + WA);
        }
        [TestMethod]
        public void TestAllNumbers4()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10)) *
                           (x - rand.Next(0, 10));
                var newexpr = expr.Expand();
                var roots = newexpr.SolveEquation(x);
                Assert.IsTrue(roots.Count > 0);
                foreach (var root in roots.FiniteSet())
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestAllNumbers4complex()
        {
            var rand = new Random(24 /* seed should be specified due to required determinism*/ );
            for (int i = 0; i < 30; i++)
            {
                var expr = (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10))) *
                           (x - (rand.Next(0, 10) + MathS.i * rand.Next(0, 10)));
                var newexpr = expr.Expand();
                foreach (var root in newexpr.SolveEquation(x).FiniteSet())
                    AssertRoots(newexpr, x, root);
            }
        }
        [TestMethod]
        public void TestVars2()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars4mp()
        {
            var goose = MathS.Var("goose");
            var eq = ((x - goose) * (x - 3) * (MathS.Sqr(x) - 4));
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars3()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - goose) * (x + goose * momo) * (x - momo * 2)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars4()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var quack = MathS.Var("quack");
            var eq = ((x - goose) * (x - momo) * (x - quack) * (x - momo * goose * quack)).Expand();
            var roots = eq.SolveEquation(x);
            Assert.IsNotNull(roots, "roots is null");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestVars2_0()
        {
            var goose = MathS.Var("goose");
            var momo = MathS.Var("momo");
            var eq = ((x - momo) * (x - goose)).Expand();
            var roots = eq.SolveEquation(x);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, x, root);
        }
        [TestMethod]
        public void TestReduce()
        {
            Entity expr = "3x5 + 5x3";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 3);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        public void InvertedFunctionTests(string func, int rootAmount)
        {
            Entity toRepl = func + "(x2 + 3)";
            Entity expr = MathS.Sqr(toRepl) + 0.3 * toRepl - 0.1 * MathS.Var("a");
            var roots = expr.SolveEquation(x);
            AssertRootCount(roots, rootAmount);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr.Substitute("a", 5), x, root.Substitute("n_1", 3).Substitute("a", 5));
        }

        [TestMethod]
        public void TestRepl1()
            => InvertedFunctionTests("sin", 8);
        [TestMethod]
        public void TestRepl2()
            => InvertedFunctionTests("cos", 8);
        [TestMethod]
        public void TestRepl3()
            => InvertedFunctionTests("tan", 4);
        [TestMethod]
        public void TestRepl4()
            => InvertedFunctionTests("cotan", 4);
        [TestMethod]
        public void TestRepl5()
            => InvertedFunctionTests("arcsin", 4);

        // TODO: arccos return analytically correct answer, but incorrect when substituting variables (as there are some Y such there is no X that arccos(X) = Y)
        //[TestMethod]
        //public void TestRepl6()
        //    => InvertedFunctionTests("arccos", 4);
        [TestMethod]
        public void TestRepl7()
            => InvertedFunctionTests("arctan", 4);
        [TestMethod]
        public void TestRepl8()
            => InvertedFunctionTests("arccotan", 4);


        private readonly List<Number> KeyPoints = new List<Number>
        {
            new Number(0, 1),
            new Number(1, 0),
            new Number(-3, -3),
            new Number(2, 2),
            new Number(13, 13),
            new Number(-9, +7),
            new Number(0.5, -0.5),
            new Number(-0.5, 0.5),
        };

        [TestMethod]
        public void TestLogs()
        {
            Entity eqs = "log(32, x) - 5";
            var roots = eqs.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eqs, x, root);
        }

        [TestMethod]
        public void TestFerrari1()
        {
            Entity eq = "x4 - x2 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari2()
        {
            Entity eq = "x4 - x + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari3()
        {
            Entity eq = "x4 - x3 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari4()
        {
            Entity eq = "x4 - x2 + x - x3 + 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }
        [TestMethod]
        public void TestFerrari5()
        {
            Entity eq = "(x2 - 1)2 - 2";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari6()
        {
            Entity eq = "x4 - 2x2 - 1";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void TestFerrari7()
        {
            Entity eq = "x4 - 2x2 - 2";
            var roots = eq.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(eq, "x", root);
        }

        [TestMethod]
        public void Test1px()
        {
            Entity expr = "pi - 1^x";
            expr.SolveEquation("x"); // Check if it doesn't hang
        }

        [TestMethod]
        public void ExpSimpl()
        {
            Entity expr = "x^4 * x^y - 2";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 1);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots1()
        {
            Entity expr = "sin(x) + cos(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots2()
        {
            Entity expr = "sin(x) + cos(x) - 0.5";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots3()
        {
            Entity expr = "sin(x) + cos(x) - 2";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 2);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }

        [TestMethod]
        public void TestLinearTrigRoots4()
        {
            Entity expr = "sin(x)^2 + cos(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 3);
            
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
            // 2 pi n, -pi/2 + 2 pi n, pi/2 + 2 pi n
        }

        [TestMethod]
        public void TestLinearTrigRoots5()
        {
            var exprs = new List<Entity>() {
                "3 * sin(2 * x + 1) - sin(x) - a",
                "3 * sin(1 + 2 * x) - sin(x) - a",
                "3 * sin(1 + x * 2) - sin(x) - a",
                "3 * sin(x * 2 + 1) - sin(x) - a",
                "3 * cos(2 * x + 1) - cos(x) - a",
                "3 * cos(1 + 2 * x) - cos(x) - a",
                "3 * cos(1 + x * 2) - cos(x) - a",
                "3 * cos(x * 2 + 1) - cos(x) - a",
            };
            foreach (var expr in exprs)
            {
                var roots = expr.SolveEquation("x");
                AssertRootCount(roots, 4);
                foreach (var root in roots.FiniteSet())
                    AssertRoots(expr, x, root);
            }
        }

        [TestMethod]
        public void TestLinearTrigRoots7()
        {
            var exprs = new List<Entity>() {
                "3 * sin(2 * x) - sin(x) - a",
                "3 * sin(x * 2) - sin(x) - a",
                "3 * sin(1 + x) - sin(x) - a",
                "3 * sin(x + 1) - sin(x) - a",
                "3 * cos(2 * x) - cos(x) - a",
                "3 * cos(x * 2) - cos(x) - a",
                "3 * cos(1 + x) - cos(x) - a",
                "3 * cos(x + 1) - cos(x) - a",
            };
            foreach (var expr in exprs)
            {
                var roots = expr.SolveEquation("x");
                foreach (var root in roots.FiniteSet())
                    AssertRoots(expr, x, root);
            }
        }

        [TestMethod]
        public void TestLinearTrigRoots6()
        {
            Entity expr = "sin(2*x + 1) - sin(x) - 1";
            var roots = expr.SolveEquation("x");
            AssertRootCount(roots, 4);
            foreach (var root in roots.FiniteSet())
                AssertRoots(expr, x, root);
        }
    }
}

