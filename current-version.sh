#!/bin/bash

currFolder="$(pwd)"
csproj=$currFolder/XKit.Lib.Host/XKit.Lib.Host.csproj
xmlstarlet sel -t -m "/Project/PropertyGroup/Version" -v . -n <$csproj
