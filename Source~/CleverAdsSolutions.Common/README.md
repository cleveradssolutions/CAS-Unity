# CleverAdsSolutions Common project
This project contains the source code for building `/Runtime/CleverAdsSolutions.Common.dll`, a shared library used in the main plugin repository. CleverAdsSolutions.Common.dll provides common structure with documentation.

## Build
To build `CleverAdsSolutions.Common.dll`, you need to specify the path to `UnityEngine.dll`. Find the path to `UnityEngine.dll` in the installed UnityEditor folder, for example, for MacOS it would be:
```
Unity.app/Contents/Managed/UnityEngine.dll
```
Use MSBuild to build `CleverAdsSolutions.Common.dll` with the ReferencePath parameter that points to the path of UnityEngine.dll:
```
msbuild CleverAdsSolutions.Common.csproj /p:ReferencePath="path\to\desired\UnityEngine.dll"
```