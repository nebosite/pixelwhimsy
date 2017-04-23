using System;
using System.Collections.Generic;
using System.Text;
using DirectVarmint;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Management;

namespace PixelWhimsy
{
    public partial class Utilities
    {
        static Random localRand = new Random();

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Safely log an exception to a standard location
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void LogException(Exception e)
        {
            try
            {
                File.AppendAllText(GlobalState.LogFileName,
                    Environment.NewLine +
                    new string('-', 80) +
                    Environment.NewLine +
                    DateTime.Now.ToString() +
                    Environment.NewLine +
                    e.ToString());
            }
            catch (Exception) { }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Set up the folders needed to store data
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void SetupDataFolders(out string picsPath, out string dataPath)
        {
            string picturePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (!Directory.Exists(picturePath)) picturePath = Directory.GetCurrentDirectory();

            picsPath = Path.Combine(picturePath, "PixelWhimsy");

            try
            {
                dataPath = Path.Combine(Directory.GetCurrentDirectory(), "screendata");
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                string testName = Path.Combine(dataPath, "testwrite.txt");
                File.WriteAllText(testName, "foo");
                File.Delete(testName);
            }
            catch (Exception)
            {
                dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "screendata");
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            }

            if (!Directory.Exists(picsPath)) Directory.CreateDirectory(picsPath);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get the first found mac address of the local machine
        /// </summary>
        /// --------------------------------------------------------------------------
        public static string GetMacAddress()
        {
            try
            {
                ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration");

                foreach (ManagementObject managementObject in query.Get())
                {
                    if ((bool)managementObject["IPEnabled"] && managementObject["MacAddress"] != null)
                    {
                        string macAddress = managementObject["MacAddress"].ToString().Replace(":","");
                        if (!macAddress.StartsWith("00000000")) return macAddress; 
                    }
                }
            }
            catch (Exception) { } // We don't care if this code succeeds

            return "NA";
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// If a color is animated, turn it into a regular non-animated color
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort Flatten(ushort color)
        {
            uint rgbColor = GlobalState.Palette[color];
            return PixelBuffer.ColorConverters._5Bit(rgbColor);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// random shortcut
        /// </summary>
        /// <param name="max">Maximum value</param>
        /// <returns>0 - max-1</returns>
        /// --------------------------------------------------------------------------
        public static int Rand(int max)
        {
            if (max <= 0) return 0;
            else return localRand.Next(max) % max;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// random shortcut for floats
        /// </summary>
        /// <param name="max">Maximum value</param>
        /// <returns>0.0 - (max - (small number))</returns>
        /// --------------------------------------------------------------------------
        public static double DRand(double max)
        {
            return ((double)localRand.Next()) / int.MaxValue * max; 
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get a meaningful, simple title from an exception by finding the method 
        /// it failed in and the type of the exception thrown.
        /// </summary>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        internal static string GetTitleFromException(Exception e)
        {
            string[] nameParts = e.GetType().ToString().Split('.');
            string shortName = nameParts[nameParts.Length - 1];

            string methodName = "UNKNOWN";

            if (e.StackTrace != null)
            {
                Match match = Regex.Match(e.StackTrace, @"(PixelWhimsy\S+?)\(");
                if (match.Success)
                {
                    nameParts = match.Groups[1].Value.Split('.');
                    methodName = nameParts[nameParts.Length - 1];
                }
            }

            return shortName + " in " + methodName;
        }

        static DateTime appStartTime = DateTime.Now;
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Thread worker for reporting exception asynchronously
        /// </summary>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        internal static void ReportExceptionWorker(object exception)
        {
            Exception e = (Exception)exception;
            ReportException(e,appStartTime.ToString() + " - " + DateTime.Now.ToString());
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Send a report about an exception
        /// </summary>
        /// <param name="e"></param>
        /// --------------------------------------------------------------------------
        internal static void ReportException(Exception e, string text)
        {
            string body = 
                Settings.Id + Environment.NewLine + text + Environment.NewLine +
                "PixelWhimsy threw this exception: " + Environment.NewLine +
                e.GetType().ToString() + Environment.NewLine + 
                e.ToString();

            ReportError(GetTitleFromException(e), body);
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Send a generic error report
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="message"></param>
        /// --------------------------------------------------------------------------
        internal static void ReportError(string errorType, string message)
        {
            try
            {
                string uriString = "http://aspx.pixelwhimsy.com/pixelwhimsy/mailer.aspx";

                WebClient myWebClient = new WebClient();
                NameValueCollection myNameValueCollection = new NameValueCollection();

                myNameValueCollection.Add("message", message);
                myNameValueCollection.Add("version", AssemblyConstants.Version);
                myNameValueCollection.Add("reporttype", errorType);

                byte[] responseArray = myWebClient.UploadValues(uriString, "POST", myNameValueCollection);
                //string response = Encoding.ASCII.GetString(responseArray);
                // if(response == "Mail Sent.") { // it worked; }
            }
            catch (Exception) { } // Don't want this to crash the program
        }
        /// --------------------------------------------------------------------------
        /// <summary>
        /// Compare two version and determine if the web version is newer
        /// </summary>
        /// <returns>String representation of the version</returns>
        /// --------------------------------------------------------------------------
        internal static bool NeedsNewVersion(string thisVersion, string otherVersion)
        {
            if (otherVersion == null) return false;

            string[] parts1 = thisVersion.Split('.');
            string[] parts2 = otherVersion.Split('.');

            for (int i = 0; i < parts1.Length && i < parts2.Length; i++)
            {
                int v1 = int.Parse(parts1[i]);
                int v2 = int.Parse(parts2[i]);
                if (v1 < v2) return true;
                if (v1 > v2) break;
            }

            return false;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Get the current version from the web site
        /// </summary>
        /// <returns>String representation of the version</returns>
        /// --------------------------------------------------------------------------
        public static string GetCurrentVersion(string url)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Timeout = 5000;

                WebResponse myResponse = webRequest.GetResponse();
                Stream inStream = myResponse.GetResponseStream();

                StreamReader reader = new StreamReader(inStream, System.Text.Encoding.ASCII);
                string output = reader.ReadToEnd();

                myResponse.Close();

                if (output.Contains("<PixelWhimsyData>"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(output);

                    XmlNode currentVersionNode = doc.SelectSingleNode("PixelWhimsyData/CurrentVersion");

                    if (currentVersionNode != null) return currentVersionNode.InnerText;
                    else return null;
                }
                else return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Advance an animated color 1 slot in its frame
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void AnimateColor(ref ushort color, uint frame)
        {
            if (color >= 0x8000)
            {
                color = (ushort)((color & 0xffC0) + (frame % 64));
            }
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Advance an animated color 1 slot in its frame
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort GetAnimatedColor(ushort color, int frame)
        {
            ushort newColor = color;
            if (color >= 0x8000)
            {
                newColor = (ushort)((color & 0xffC0) + (frame % 64));
            }

            return newColor;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Given two vectors, give the postive or negative direction change required
        /// to align vector 1 with vector2
        /// </summary>
        /// <returns>-1 or 1</returns>
        /// --------------------------------------------------------------------------
        public static  int GetDirectionChange(double v1x, double v1y, double v2x, double v2y)
        {
            double angle = GetAngle(v1x, v1y, v2x, v2y);
            return angle > 0 ? 1 : -1;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Return the angle between two vectors
        /// </summary>
        /// --------------------------------------------------------------------------
        public static double GetAngle(double v1x, double v1y, double v2x, double v2y)
        {
            double d1 = Math.Sqrt(v1x * v1x + v1y * v1y);
            double d2 = Math.Sqrt(v2x * v2x + v2y * v2y);

            double theta2 = Math.Acos(v2y / d2);
            if (v2x < 0) theta2 = Math.PI * 2 - theta2;

            double theta1 = Math.Acos(v1y / d1);
            if (v1x < 0) theta1 = Math.PI * 2 - theta1;

            if (theta2 - theta1 > Math.PI) theta1 += Math.PI * 2;
            if (theta1 - theta2 > Math.PI) theta2 += Math.PI * 2;

            return theta2 - theta1;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Take and reformat a string to fit withing the specified maxwidth
        /// </summary>
        /// --------------------------------------------------------------------------
        public static string BreakString(PixelBuffer.DVFont font, int maxWidth, string text)
        {
            string[] parts = text.Split(' ');
            List<string> lines = new List<string>();

            string newLine = "";
            foreach (string part in parts)
            {
                string longerLine = newLine + (newLine == "" ? "" : " ") + part;
                int realWidth = (int)(font.Measure(longerLine).Width);
                if (realWidth > maxWidth)
                {
                    if (newLine == "")
                    {
                        lines.Add(longerLine);
                        newLine = "";
                    }
                    else
                    {
                        lines.Add(newLine);
                        newLine = part;
                    }
                }
                else newLine = longerLine;
            }

            if (newLine != "") lines.Add(newLine);

            return string.Join("\n", lines.ToArray());
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Pick a random color, not black
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort PickRandomColor(DVWindow dvWindow, bool allowBlack)
        {
            return dvWindow.MainBuffer.GetPaletteColor(PickRandomRGBColor(allowBlack, 7, 3, 4));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Pick a random color in the dark ranges
        /// </summary>
        /// --------------------------------------------------------------------------
        public static ushort PickDarkRandomColor(DVWindow dvWindow, bool allowBlack)
        {
            return dvWindow.MainBuffer.GetPaletteColor(PickRandomRGBColor(allowBlack, 13, 4, 0));
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Pick a random color, not black
        /// </summary>
        /// --------------------------------------------------------------------------
        public static uint PickRandomRGBColor(bool allowBlack, int width, int randPart, int offset)
        {
            if (randPart + offset > width) width = randPart + offset;
            randPart++;
            int multiplier = 255 / (width);
            uint colorBits = (uint)(Rand(7) + 1);
            if (allowBlack) colorBits = (uint)(Rand(8));
            uint realColor = 0;
            if ((colorBits & 0x04) > 0) realColor += (uint)(((Rand(randPart) + offset) * multiplier) << 16);
            if ((colorBits & 0x02) > 0) realColor += (uint)(((Rand(randPart) + offset) * multiplier) << 8);
            if ((colorBits & 0x01) > 0) realColor += (uint)(((Rand(randPart) + offset) * multiplier) << 0);

            return realColor;
        }

        /// --------------------------------------------------------------------------
        /// <summary>
        /// Find out the color targets from a regular DVWindow color
        /// </summary>
        /// --------------------------------------------------------------------------
        public static void GetColorTargets(ushort color, out int redTarget, out int greenTarget, out int blueTarget)
        {
            redTarget = (color >> 10) & 0x1f;
            greenTarget = (color >> 5) & 0x1f;
            blueTarget = (color) & 0x1f;
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Helper method for distance
        /// </summary>
        /// --------------------------------------------------------------------------
        protected static double GetDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }


        /// --------------------------------------------------------------------------
        /// <summary>
        /// Methods to generate perlin noise
        /// </summary>
        /// --------------------------------------------------------------------------
        public static class Noise
        {
            static int noiseWidth = 2000;
            static int noiseHeight = 2000;
            static int noiseDepth = 2000;

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Base level noise function
            /// </summary>
            /// --------------------------------------------------------------------------
            public static float Noise3D(int x, int y, int z)
            {
                int v = x + y * 57 + z * 3461;
                ulong n = (ulong)(v > 0 ? v : -v);
                n = ((n << 13) ^ n) & 0xffffffff;
                ulong factor = (((n * n) & 0xffffffff) * 15731 + 789221) & 0xffffffff;
                float noise = ((n * (factor) + 1376312589) & 0xffffffff) / 4294967295.0f;

                return noise;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Continues interpolated noise
            /// </summary>
            /// --------------------------------------------------------------------------
            public static float InterpolatedNoise_3D(float x, float y, float z)
            {

                // normalize negative components
                if (x < 0) x += ((int)(x / noiseWidth) - 1) * -noiseWidth;
                if (y < 0) y += ((int)(y / noiseHeight) - 1) * -noiseHeight;
                if (z < 0) z += ((int)(z / noiseDepth) - 1) * -noiseDepth;

                //get fractional part of x and y
                float fractX = (x) - (int)(x);
                float fractY = (y) - (int)(y);
                float fractZ = (z) - (int)(z);

                //wrap around
                int x1 = ((int)(x) + noiseWidth) % noiseWidth;
                int y1 = ((int)(y) + noiseHeight) % noiseHeight;
                int z1 = ((int)(z) + noiseDepth) % noiseDepth;

                //neighbor values
                int x2 = (x1 + noiseWidth - 1) % noiseWidth;
                int y2 = (y1 + noiseHeight - 1) % noiseHeight;
                int z2 = (z1 + noiseDepth - 1) % noiseDepth;

                //smooth the noise with bilinear interpolation
                float value = 0.0f;
                value += fractX * fractY * fractZ * Noise3D(x1, y1, z1);
                value += fractX * (1 - fractY) * fractZ * Noise3D(x1, y2, z1);
                value += (1 - fractX) * fractY * fractZ * Noise3D(x2, y1, z1);
                value += (1 - fractX) * (1 - fractY) * fractZ * Noise3D(x2, y2, z1);

                value += fractX * fractY * (1 - fractZ) * Noise3D(x1, y1, z2);
                value += fractX * (1 - fractY) * (1 - fractZ) * Noise3D(x1, y2, z2);
                value += (1 - fractX) * fractY * (1 - fractZ) * Noise3D(x2, y1, z2);
                value += (1 - fractX) * (1 - fractY) * (1 - fractZ) * Noise3D(x2, y2, z2);

                return value;
            }

            /// --------------------------------------------------------------------------
            /// <summary>
            /// Perlin noise generator
            /// </summary>
            /// --------------------------------------------------------------------------
            public static float PerlinNoise_3D(double x, double y, double z)
            {
                float total = 0;
                float persistence = 0.5f;
                int octaves = 8;

                for (int i = 0; i < octaves; i++)
                {
                    float frequency = 1 << i;
                    float amplitude =  (float)Math.Pow(persistence, i);
                    float noise = InterpolatedNoise_3D((float)(x * frequency), (float)(y * frequency), (float)(z * frequency));
                    total += noise * amplitude;
                }
                total -= 0.5f;
                if (total < 0) total = 0;
                if (total >= 1.0) total = 0.9999f;
                return total;
            }
        }


    }
}
