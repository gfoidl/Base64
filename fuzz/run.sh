#!/bin/bash

set -e

if [[ $# -lt 2 ]]; then
    echo "first arg must be duration for timeout"
    echo "seconds arg must be the fuzz-function (see Program.cs)"
    exit 1
fi

lib="gfoidl.Base64.dll"
tfm="netcoreapp3.0"
path="gfoidl.Base64.FuzzTests/bin/Release/$tfm"

cp ./instrumented/$lib "$path"/

duration=$1
shift

echo "running fuzz for $duration..."
timeout --preserve-status "$duration" afl-fuzz -i testcases -o findings -m 10000 -t 5000 dotnet "$path"/gfoidl.Base64.FuzzTests.dll "$*"

# when there are any reports in ./findings/crashes, so there are failures
if [[ $(ls ./findings/crashes | wc -l) -gt 0 ]]; then
    cd findings/crashes

    rename 's|:|-|g' ./*
    ls -la
    
    exit 1
fi
