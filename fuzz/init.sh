#!/bin/bash

set -e

echo "See https://github.com/Metalnem/sharpfuzz for setup"
echo ""

lib=gfoidl.Base64.dll

dotnet build -c Release gfoidl.Base64.FuzzTests
mkdir -p ./instrumented
cp ../source/gfoidl.Base64/bin/Release/netcoreapp3.0/$lib ./instrumented/$lib

sharpfuzz ./instrumented/$lib

echo "$lib instrumented and ready to go"
