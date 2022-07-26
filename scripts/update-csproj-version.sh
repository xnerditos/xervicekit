#!/bin/bash

if [[ $# -ne 2 ]]; then
  echo Specify csproj file and new version number
fi

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")

csproj="$1"
version="$2"

xmlstarlet edit -L -u "/Project/PropertyGroup/Version" -v "$version" $csproj
xmlstarlet edit -L -u "/Project/PropertyGroup/AssemblyVersion" -v "$version.0" $csproj


