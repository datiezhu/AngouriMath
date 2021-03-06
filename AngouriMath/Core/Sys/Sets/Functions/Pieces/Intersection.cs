﻿
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using Edge1D = System.Tuple<double, bool>;
using Edge = System.Tuple<AngouriMath.Entity, bool, bool>;


/*
 *
 * INTERSECTION
 *
 */
namespace AngouriMath.Core.Sets
{
    static partial class PieceFunctions
    {
        internal static Edge1D IntersectEdge(
            double num1, bool closed1,
            double num2, bool closed2,
            Func<double, double, bool> comparator
            )
        {
            double res;
            bool closed;
            if (num2 == num1)
            {
                res = num1;
                closed = closed1 && closed2; // if at least one is open, result is open
            }
            else if (comparator(num1, num2))
            {
                res = num1;
                closed = closed1;
            }
            else
            {
                res = num2;
                closed = closed2;
            }

            return new Edge1D(res, closed);
        }

        internal static Tuple<Edge1D, Edge1D> IntersectAxis(
            double min1, bool closedMin1,
            double max1, bool closedMax1,
            double min2, bool closedMin2,
            double max2, bool closedMax2)
            => new Tuple<Edge1D, Edge1D>(
                IntersectEdge(
                    min1, closedMin1,
                    min2, closedMin2,
                    (min1, min2) => min1 > min2
                ),
                IntersectEdge(
                    max1, closedMax1,
                    max2, closedMax2,
                    (max1, max2) => max1 < max2
                    )
                );

        private static Tuple<Edge, Edge> SortEdges(Edge A, Edge B)
        {
            var num1 = A.Item1.Eval();
            var num2 = B.Item1.Eval();
            double lowRe = num1.Re;
            double upRe = num2.Re;
            double lowIm = num1.Im;
            double upIm = num2.Im;
            bool lowReClosed = A.Item2;
            bool upReClosed = B.Item2;
            bool lowImClosed = A.Item3;
            bool upImClosed = B.Item3;
            if (lowRe > upRe)
            {
                Const.Swap(ref lowRe, ref upRe);
                Const.Swap(ref lowReClosed, ref upReClosed);
            }

            if (lowIm > upIm)
            {
                Const.Swap(ref lowIm, ref upIm);
                Const.Swap(ref lowImClosed, ref upImClosed);
            }

            return new Tuple<Edge, Edge>(
                new Edge(
                    new Number(lowRe, lowIm),
                    lowReClosed,
                    lowImClosed
                    ),
                new Edge(
                    new Number(upRe, upIm),
                    upReClosed,
                    upImClosed
                    )
                );
        }

        /// <summary>
        /// Unsafe!
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static Piece Intersect(Piece A, Piece B)
        {
            if (A == B)
                return A;
            var edgesASorted = SortEdges(A.LowerBound(), A.UpperBound());
            var edgesBSorted = SortEdges(B.LowerBound(), B.UpperBound());
            var low1 = edgesASorted.Item1;
            var low2 = edgesBSorted.Item1;
            var up1 = edgesASorted.Item2;
            var up2 = edgesBSorted.Item2;
            var low1Num = low1.Item1.Eval();
            var low2Num = low2.Item1.Eval();
            var up1Num = up1.Item1.Eval();
            var up2Num = up2.Item1.Eval();
            if (low1Num.Re > up2Num.Re || (low1Num.Re == up2Num.Re && (!low1.Item2 || !up2.Item2)) ||
                low2Num.Re > up1Num.Re || (low2Num.Re == up1Num.Re && (!low2.Item2 || !up1.Item2)) ||
                low1Num.Im > up2Num.Im || (low1Num.Im == up2Num.Im && (!low1.Item3 || !up2.Item3)) ||
                low2Num.Im > up1Num.Im || (low2Num.Im == up1Num.Im && (!low2.Item3 || !up1.Item3)))
                return null; // if they don't intersect
            var realAxis = IntersectAxis(
                low1Num.Re, low1.Item2,
                up1Num.Re, up1.Item2,
                low2Num.Re, low2.Item2,
                up2Num.Re, up2.Item2
            );
            var imaginaryAxis = IntersectAxis(
                low1Num.Im, low1.Item3,
                up1Num.Im, up1.Item3,
                low2Num.Im, low2.Item3,
                up2Num.Im, up2.Item3
            );
            var edgeLeft = new Edge(
                new Number(realAxis.Item1.Item1, imaginaryAxis.Item1.Item1),
                realAxis.Item1.Item2,
                imaginaryAxis.Item1.Item2);
            var edgeRight = new Edge(
                new Number(realAxis.Item2.Item1, imaginaryAxis.Item2.Item1),
                realAxis.Item2.Item2,
                imaginaryAxis.Item2.Item2);
            var res = Piece.Interval(edgeLeft, edgeRight);
            return res;
        }
    }
}