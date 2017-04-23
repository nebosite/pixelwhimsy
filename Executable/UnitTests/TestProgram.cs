using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using NUnit.Framework;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Threading;

namespace PixelWhimsy
{
    static partial class Program
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Tests for the Utilities class
        /// </summary>
        /// --------------------------------------------------------------------------
        [TestFixture]
        public class Test
        {
            List<Exception> loggedExceptions = new List<Exception>();

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Use this to test exception logging in RunWithErrorHandling
            /// </summary>
            /// --------------------------------------------------------------------------
            void testLogger(object e)
            {
                loggedExceptions.Add((Exception)e);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test setup
            /// </summary>
            /// --------------------------------------------------------------------------
            [SetUp]
            public void Setup()
            {
                Settings.ReportErrors = false;
                loggedExceptions.Clear();
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test RunWithErrorHandling
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestRunWithErrorHandling()
            {
                GlobalState.Debugging = false;
                FrameDriver testDriver = delegate(int rate)
                {
                    Assert.AreEqual(11, rate);
                };

                Program.RunWithErrorHandling(testDriver, testLogger, 11, 5, 10000);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test RunWithErrorHandling
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestRunWithErrorHandling_ErrorTolerant()
            {
                GlobalState.Debugging = false;
                Settings.ReportErrors = true;
                int callCount = 0;
                FrameDriver testDriver = delegate(int rate)
                {
                    callCount++;
                    if (callCount < 3)
                    {
                        throw new Exception("Blah");
                    }

                    Assert.AreEqual(12, rate);
                };

                Program.RunWithErrorHandling(testDriver, testLogger, 12, 5, 10000);
                Thread.Sleep(200);
                Assert.AreEqual(3, callCount);
                Assert.AreEqual(2, loggedExceptions.Count);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test RunWithErrorHandling
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestRunWithErrorHandling_FailOnTooMany()
            {
                GlobalState.Debugging = false;
                Settings.ReportErrors = false;
                int callCount = 0;
                FrameDriver testDriver = delegate(int rate)
                {
                    callCount++;
                    if (callCount < 30)
                    {
                        throw new Exception("Blah");
                    }
                };

                try
                {
                    Program.RunWithErrorHandling(testDriver, testLogger, 12, 4, 10000);
                    Assert.Fail("Should Throw");
                }
                catch (ApplicationException) { }

                Assert.AreEqual(4, callCount);
                Assert.AreEqual(0, loggedExceptions.Count);
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test RunWithErrorHandling
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestRunWithErrorHandling_TolerateIntermittantFailures()
            {
                Settings.ReportErrors = true;
                int callCount = 0;
                FrameDriver testDriver = delegate(int rate)
                {
                    callCount++;
                    if (callCount < 5)
                    {
                        Thread.Sleep(50);
                        throw new Exception("Blah");
                    }
                };

                Program.RunWithErrorHandling(testDriver, testLogger, 12, 3, 50);

                Thread.Sleep(200);

                Assert.AreEqual(5, callCount);
                Assert.AreEqual(4, loggedExceptions.Count);
            }

            ///// --------------------------------------------------------------------------
            ///// <summary>
            ///// Test GetResolution
            ///// </summary>
            ///// --------------------------------------------------------------------------
            //[Test]
            //public void TestGetResolution()
            //{
            //    AssertResolution(816, 612, 1, 1024, 768, PixelCount.High);
            //    AssertResolution(632, 474, 1, 1024, 768, PixelCount.Medium);
            //    AssertResolution(412, 309, 2, 1024, 768, PixelCount.Low);
            //    AssertResolution(816, 612, 1, 1280, 1024, PixelCount.High);
            //    AssertResolution(564, 423, 2, 1280, 1024, PixelCount.Medium);
            //    AssertResolution(488, 366, 2, 1280, 1024, PixelCount.Low);
            //    AssertResolution(700, 525, 2, 1600, 1200, PixelCount.High);
            //    AssertResolution(632, 474, 2, 1600, 1200, PixelCount.Medium);
            //    AssertResolution(464, 348, 3, 1600, 1200, PixelCount.Low);
            //    AssertResolution(700, 525, 2, 1920, 1200, PixelCount.High);
            //}

            ///// --------------------------------------------------------------------------
            ///// <summary>
            ///// Helper for testing GetResolution
            ///// </summary>
            ///// --------------------------------------------------------------------------
            //private void AssertResolution(int expx, int expy, int expm, int w, int h, PixelCount pixelCount)
            //{
            //    int resx, resy, multiplier;
            //    Program.GetResolution(w, h, pixelCount, out resx, out resy, out multiplier);
            //    string label = w + "x" + h + ", " + pixelCount;
            //    Console.WriteLine(resx + ", " + resy + ", " + multiplier);
            //    Assert.AreEqual(expx, resx, "\n Bad resx on " + label );
            //    Assert.AreEqual(expy, resy, "\n Bad resy on " + label );
            //    Assert.AreEqual(expm, multiplier, "\n Bad multiplier on " + label );
            //}
        }
    }
}
