#!/bin/bash

echo "See https://github.com/Metalnem/sharpfuzz for setup"

lib=gfoidl.Base64.dll

mkdir ./instrumented
cp ../source/gfoidl.Base64/bin/Release/netcoreapp3.0/$lib ./instrumented/$lib

sharpfuzz ./instrumented/$lib
