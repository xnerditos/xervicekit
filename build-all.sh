#!/bin/bash

currFolder="$(pwd)"  # Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")  # Absolute path this script is in
scriptDir=$(dirname "$scriptPath")
cd $scriptDir

function dobuild() {
    cd "$projdir"
    echo
    echo =============================================================
    echo Building $projdir
    echo =============================================================
    echo
    #dotnet build "$projdir.csproj" 
    dotnet build 
    cd ..
}

# Build main assemblies
ls XKit.Lib.* -d | while read -r projdir
do
    dobuild
done

# Build simple tests
projdir=UnitTests
dobuild

# Build complex tests
cd $scriptDir/SystemTests
ls -d */ | while read -r projdir
do
    dobuild
done

cd $currFolder
