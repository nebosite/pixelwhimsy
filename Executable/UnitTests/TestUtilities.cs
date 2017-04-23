using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using NUnit.Framework;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace PixelWhimsy
{
    public class FooBarException : System.Exception
    {

    }

    public partial class Utilities
    {

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Tests for the Utilities class
        /// </summary>
        /// --------------------------------------------------------------------------
        [TestFixture]
        public class Test
        {

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Helpers  for exception stack testing
            /// </summary>
            /// --------------------------------------------------------------------------
            void FailTest1() { FailTest2(); }
            void FailTest2() { throw new FooBarException(); }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// We should be able to boild exceptions down to a meaningful title
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestGetTitleFromException()
            {
                try
                {
                    FailTest1();
                }
                catch (FooBarException e)
                {
                    Assert.AreEqual("FooBarException in FailTest2", Utilities.GetTitleFromException(e));
                }

                Assert.AreEqual("Exception in UNKNOWN", Utilities.GetTitleFromException(new Exception("flf")));
            }


            //[Test]
            //public void TestSendMail()
            //{
            //    Foo2();
            //}

            ///// --------------------------------------------------------------------------
            ///// <summary>
            ///// Test Version of sendmial code
            ///// </summary>
            ///// --------------------------------------------------------------------------
            //void Foo2()
            //{
            //    string uriString = "http://aspx.pixelwhimsy.com/mailer.aspx";

            //    WebClient myWebClient = new WebClient();
            //    NameValueCollection myNameValueCollection = new NameValueCollection();

            //    myNameValueCollection.Add("name", "Eric J");
            //    myNameValueCollection.Add("email", "foo@bar.com");

            //    Exception e = new Exception("Hi there");
            //    myNameValueCollection.Add("message", "PixelWhimsy threw an exception:\r\n" + e.ToString());
            //    myNameValueCollection.Add("version", AssemblyConstants.Version);
            //    myNameValueCollection.Add("reporttype", "Bad Foo");
 
            //    Console.WriteLine("\nUploading to {0} ...", uriString);

            //    byte[] responseArray = myWebClient.UploadValues(uriString, "POST", myNameValueCollection);
            //    Console.WriteLine("\nResponse received was:\n{0}", Encoding.ASCII.GetString(responseArray));
            //}

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Get the current version from the web site
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestGetCurrentVersion()
            {
                Assert.AreEqual(null, Utilities.GetCurrentVersion("http://www.NON-Existent-pixelwhimsy.com/CurrentVersion.xml"));
                Assert.AreEqual("0.3.0.0", Utilities.GetCurrentVersion("http://version.pixelwhimsy.com/TestCurrentVersion.xml"));
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Compare two versions and determine if we need an update
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestNeedsUpdate()
            {
                Assert.AreEqual(false, Utilities.NeedsNewVersion(AssemblyConstants.Version, null));
                Assert.AreEqual(false, Utilities.NeedsNewVersion(AssemblyConstants.Version, "0.0.0.9999"));
                Assert.AreEqual(false, Utilities.NeedsNewVersion(AssemblyConstants.Version, "0.1.0009.0"));
                Assert.AreEqual(true, Utilities.NeedsNewVersion(AssemblyConstants.Version, "90.1.0009.0"));
                Assert.AreEqual(true, Utilities.NeedsNewVersion(AssemblyConstants.Version, "99"));
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Test GetDirectionChange
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestGetDirectionChange()
            {
                Assert.AreEqual(1, Utilities.GetDirectionChange(0, 1, 1, 1), "\nFailed on 1");
                Assert.AreEqual(1, Utilities.GetDirectionChange(0, 1, 1, -1), "\nFailed on 2");
                Assert.AreEqual(1, Utilities.GetDirectionChange(0, 1, 0, -1), "\nFailed on 3");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(0, 1, -.00001, -1), "\nFailed on 4");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(0, 1, -1, 0), "\nFailed on 5");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(0, 1, -.00001, 1), "\nFailed on 6");

                Assert.AreEqual(-1, Utilities.GetDirectionChange(1, 1, .1, 1), "\nFailed on 7");
                Assert.AreEqual(1, Utilities.GetDirectionChange(1, 1, 1, .1), "\nFailed on 8");
                Assert.AreEqual(1, Utilities.GetDirectionChange(1, 1, -.9, -1), "\nFailed on 9");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(1, 1, -1.1, -1), "\nFailed on 10");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(1, 1, -1, 2), "\nFailed on 11");


                Assert.AreEqual(-1, Utilities.GetDirectionChange(-1, 1, -1, 1), "\nFailed on 12");
                Assert.AreEqual(1, Utilities.GetDirectionChange(-1, 1, -1, 2), "\nFailed on 13");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(-1, 1, -1, .5), "\nFailed on 14");
                Assert.AreEqual(1, Utilities.GetDirectionChange(-1, 1, 1 , -.5), "\nFailed on 15");
                Assert.AreEqual(-1, Utilities.GetDirectionChange(-1, 1, 1, -1.5), "\nFailed on 16");
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Make sure we can properly reformat a string
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestBreakString()
            {
                PixelBuffer.DVFont font = new PixelBuffer.DVFont("Courier New", 10, System.Drawing.FontStyle.Regular, false);

                Assert.AreEqual("No breaks",
                    Utilities.BreakString(font, 1000, "No breaks"));

                Assert.AreEqual("Break\nsentence\ninto small\npieces.",
                    Utilities.BreakString(font, 100, "Break sentence into small pieces."));

                Assert.AreEqual("Firstpartistoobig\nfor your\nshirt.",
                    Utilities.BreakString(font, 100, "Firstpartistoobig for your shirt."));
            }
        }

    }
}
