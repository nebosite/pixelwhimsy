using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace PixelWhimsy
{
    public partial class MadLib
    {
        [TestFixture]
        public class Tests
        {
            [Test]
            public void TestNoun()
            {
                MadLib testMe = new MadLib();

                for (int j = 0; j < 20; j++)
                {
                    Console.WriteLine(testMe.GetString("Pa: @NOUNSUBJECTIVEPERSONALPRONOUN.1 @VERBTRANSITIVE^0!"));
                    Console.WriteLine(testMe.GetString("Pr: @NOUNSUBJECTIVEPERSONALPRONOUN.1 @VERBINTRANSITIVE!^1.1"));
                    Console.WriteLine(testMe.GetString("Fu: @NOUNSUBJECTIVEPERSONALPRONOUN.1 @VERBINTRANSITIVE^2!.1"));
                }
            }
            [Test]
            public void TestGeneral()
            {
                MadLib testMe = new MadLib();

                for (int i = 0; i < MadLib.MadlibSentance.Length; i++)
                {
                    Console.WriteLine(i.ToString());
                    for (int j = 0; j < 10; j++)
                    {
                        Console.WriteLine(testMe.GetString(MadLib.MadlibSentance[i]));
                    }
                }
            }
        }
    }
}
