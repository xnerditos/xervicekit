#!/bin/bash

currFolder="$(pwd)"  # Absolute path to this script, e.g. /home/user/bin/foo.sh
scriptPath=$(readlink -f "$0")  # Absolute path this script is in
scriptDir=$(dirname "$scriptPath")
cd $scriptDir

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
