#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")
cd $rootDir

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
cd $rootDir/SystemTests
ls -d */ | while read -r projdir
do
    dobuild
done

# Build tutorials
cd $rootDir/tutorials
ls -d */ | while read -r projdir
do
    dobuild
done

# Build samples
cd $rootDir/samples
ls -d */ | while read -r projdir
do
    dobuild
done

cd $currFolder
