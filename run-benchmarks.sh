#!/bin/bash
#------------------------------------------------------------------------------
set -e
#------------------------------------------------------------------------------
if [[ "$1" == "--install-sdk" ]]; then
    echo "installing SDK $SDK_VERSION"
    echo ""

    curl -o dotnet-install.sh https://dot.net/v1/dotnet-install.sh
    mkdir dotnet
    chmod u+x ./dotnet-install.sh
    ./dotnet-install.sh --install-dir $(pwd)/dotnet -v $SDK_VERSION
    rm $(pwd)/dotnet-install.sh
    export PATH="$(pwd)/dotnet:$PATH"

    echo ""
fi

echo 'installed sdks:'
dotnet --list-sdks
echo "-------------------------------------------------"

cd perf/gfoidl.Base64.Benchmarks
dotnet build -c Release
cd bin/Release/netcoreapp3.0
dotnet gfoidl.Base64.Benchmarks.dll --list tree
dotnet gfoidl.Base64.Benchmarks.dll -f *Base64EncoderBenchmark*
dotnet gfoidl.Base64.Benchmarks.dll -f *Base64UrlEncoderBenchmark*
dotnet gfoidl.Base64.Benchmarks.dll -f *DecodeStringBenchmark*
dotnet gfoidl.Base64.Benchmarks.dll -f *DecodeUtf8Benchmark*
dotnet gfoidl.Base64.Benchmarks.dll -f *EncodeStringBenchmark*
dotnet gfoidl.Base64.Benchmarks.dll -f *EncodeUtf8Benchmark*
