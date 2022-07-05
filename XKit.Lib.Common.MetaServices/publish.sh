#!/bin/bash

scriptPath=$(readlink -f "$0") # Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptDir=$(dirname "$scriptPath")  # Absolute path this script is in
projName=$(basename "$scriptDir")
projFile="$projName.csproj"

echo 
echo 
echo ====================================================================
echo $projName 
echo ====================================================================

if [[ -e ~/.nuget/api-keys/master ]]; then
    apikey=$(cat ~/.nuget/api-keys/master)
	echo Found master key!
else 
    if [[ -e ~/.nuget/api-keys/xkitlib ]]; then
        apikey=$(cat ~/.nuget/api-keys/xkitlib)
        echo Found xkitlib key!
    else 
        echo No key found for pushing nuget packages! 
        exit;
    fi
fi
publishDir=${scriptDir}/bin/Publish
mkdir -p $publishDir
rm ${publishDir}/*.nupkg &> /dev/null

target="$projFile"
echo 
echo -------------------------------------
echo Restoring "$target"
echo 
dotnet restore "$target" 
echo 
echo -------------------------------------
echo Building "$target"
echo 
dotnet build "$target" -c Release --no-restore 
echo 
echo -------------------------------------
echo Packing "$target"
echo 
dotnet pack "$target" --include-symbols --no-build -c Release -o $publishDir 
echo 
echo -------------------------------------
echo Pushing "$target"
echo 
dotnet nuget push ${publishDir}/*.nupkg -k $apikey --source https://api.nuget.org/v3/index.json --symbol-source https://symbols.nuget.org/download/symbols -sk $apikey
echo 
#echo -------------------------------------
#echo Pushing "$target" symbols
#echo 
#dotnet nuget push ${publishDir}/*.snupkg -k $apikey --source https://symbols.nuget.org/download/symbols


