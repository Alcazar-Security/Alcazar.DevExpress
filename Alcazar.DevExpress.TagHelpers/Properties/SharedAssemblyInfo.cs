using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Shared assembly versions for Alcazar.DevExpress

// Set your AssemblyVersion to {major}.0.0 - increment for breaking changes and DB schema changes
// Set your FileVersion to {major}.{minor}.{patch} - increment minor for new methods etc, increment patch for fixes without signature changes
// Set your InformationalVersion to the full Semantic Version of {major}.{minor}.{patch}-{tag}+{BuildRevision}.{BuildDate:yyyyMMdd} - increment for every deployment

[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.1")]
#if DEBUG
[assembly: AssemblyInformationalVersion("1.0.1-20250512")]
#else
[assembly: AssemblyInformationalVersion("1.0.1")]
#endif
