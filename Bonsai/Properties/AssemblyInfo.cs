using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Expose internals to 64-bit bootstrapper assembly
[assembly: InternalsVisibleTo("Bonsai64")]

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Bonsai")]
[assembly: AssemblyDescription("A visual programming language for data stream processing built on top of Rx for .NET.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Gonçalo Lopes")]
[assembly: AssemblyProduct("Bonsai")]
[assembly: AssemblyCopyright("Copyright © Gonçalo Lopes 2011-2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("2a690a30-dbde-4dc5-bd72-fa9cc4351dc2")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyInformationalVersion("3.0.0")]
