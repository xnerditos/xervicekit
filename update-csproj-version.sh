#!/bin/bash

if [[ $# -ne 2 ]]; then
echo Specify csproj file and new version number
fi

csproj="$1"
version="$2"

xmlstarlet edit -L -u "/Project/PropertyGroup/Version" -v "$version" $csproj
xmlstarlet edit -L -u "/Project/PropertyGroup/AssemblyVersion" -v "$version.0" $csproj

