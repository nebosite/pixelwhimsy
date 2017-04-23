using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;

namespace PixelWhimsy
{
    /// --------------------------------------------------------------------------
    /// <summary>
    /// Slate Tests
    /// </summary>
    /// --------------------------------------------------------------------------
    public partial class Slate : Form
    {
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Test Class
        /// </summary>
        /// --------------------------------------------------------------------------
        [TestFixture]
        public class Test
        {
            /// --------------------------------------------------------------------------
            /// <summary>
            /// Make sure we can serialize usage data
            /// </summary>
            /// --------------------------------------------------------------------------
            [Test]
            public void TestSerializeLog()
            {
                Slate testSlate = new Slate();
                testSlate.keypressLog[Keys.B] = 22;
                testSlate.keypressLog[Keys.Scroll] = 99;
                testSlate.frame = 119;
                testSlate.stressModeFrames = 10;
                testSlate.animationLog[typeof(Animation.Firework)] = 11;
                testSlate.animationLog[typeof(Animation.Moire)] = 776;

                string expectedXml = "<PixelWhimsyData>" +
                    "<TotalFrames>109</TotalFrames>" + 
                    "<KeyPresses>" +
                    "<Key><Type>B</Type><Value>22</Value></Key>" +
                    "<Key><Type>Scroll</Type><Value>99</Value></Key>" +
                    "</KeyPresses>" +
                    "<Animations>" +
                    "<Animation><Type>Firework</Type><Value>11</Value></Animation>" +
                    "<Animation><Type>Moire</Type><Value>776</Value></Animation>" +
                    "</Animations>" +
                    "</PixelWhimsyData>";

                Assert.AreEqual(expectedXml, testSlate.SerializeLog());
            }
        }
    }
}
