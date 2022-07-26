#!/bin/bash

currFolder="$(pwd)"  
scriptPath=$(readlink -f "$0")  
scriptDir=$(dirname "$scriptPath")
rootDir=$(dirname "$scriptDir")
cd $rootDir

function dotest() {
    cd "$projdir"
    echo
    echo =============================================================
    echo Testing $projdir
    echo =============================================================
    echo
    dotnet test --no-build
    cd ..
}


projdir=UnitTests
dotest

cd SystemTests
ls * -d | while read -r projdir
do
    if [[ "$projdir" != "systest-template" && -d "$projdir" ]]; then
        dotest $projdir
    fi
done

cd $currFolder
