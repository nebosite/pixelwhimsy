using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using System.Text.RegularExpressions;
using System.Globalization;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("PixelWhimsy")]
[assembly: AssemblyDescription("Pixels and Humans coexist as friends")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("NiftiBits")]
[assembly: AssemblyProduct("PixelWhimsy")]
[assembly: AssemblyCopyright("Copyright 2007")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8d54704b-2fdb-4735-a9f5-3cec5e04e966")] 

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.3.0.0013")]
[assembly: AssemblyFileVersion("1.3.0.0013")]

/// --------------------------------------------------------------------------
/// <summary>
/// Constants for the general assembly
/// </summary>
/// --------------------------------------------------------------------------
public static class AssemblyConstants
{
    public static DateTime expirationDate = DateTime.Parse("6/7/2017 8:39:20 PM",
        new CultureInfo("en-US"));
    public static string Version;
    public static string Guid = "8d54704b-2fdb-4735-a9f5-3cec5e04e966";

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Static Constructor
    /// </summary>
    /// --------------------------------------------------------------------------
    static AssemblyConstants()
    {
        Assembly thisAssembly = Assembly.GetExecutingAssembly();
        string versionText = thisAssembly.FullName.ToString();
        Version = "??.??.??";
        Match versionMatch = Regex.Match(versionText, "Version=(.*?),", RegexOptions.IgnoreCase);
        if (versionMatch.Success)
        {
            Version = versionMatch.Groups[1].Value;
        }
    }

    /// --------------------------------------------------------------------------
    /// <summary>
    /// Returns true if this assembly has expired
    /// </summary>
    /// --------------------------------------------------------------------------
    public static double DaysLeftToExpiration()
    {
#if DEBUG
        TimeSpan timeSpan = expirationDate - DateTime.Now;
        return timeSpan.TotalDays;
#else
        return 100000;
#endif
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         
