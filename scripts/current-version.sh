#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")

currFolder="$(pwd)"
csproj=$rootDir/XKit.Lib.Host/XKit.Lib.Host.csproj
xmlstarlet sel -t -m "/Project/PropertyGroup/Version" -v . -n <$csproj
