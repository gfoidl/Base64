#include "avx.h"
#include "sse.h"
#include <iostream>
#include <chrono>
//-----------------------------------------------------------------------------
using std::cout;
using std::endl;
using Clock = std::chrono::steady_clock;
using std::chrono::time_point;
using std::chrono::duration_cast;
using std::chrono::nanoseconds;
using namespace std::literals::chrono_literals;
//-----------------------------------------------------------------------------
void run()
{
    const int iterationCount = 10;
    const size_t dataSize    = 1000000;
    const size_t base64Size  = (dataSize + 2) / 3 * 4;
    
    char* data = new char[dataSize];
    char* base64 = new char[base64Size];

    size_t i = 0;
    for (; i < base64Size - 4; i += 4)
    {
        base64[i + 0] = 'A';
        base64[i + 1] = '/';
        base64[i + 2] = '+';
        base64[i + 3] = '9';
    }
    for (; i < base64Size; ++i)
        base64[i] = 'B';

    size_t written = 0;
    time_point<Clock> avx_start = Clock::now();
    for (int k = 0; k < iterationCount; ++k)
        avx_decode(base64, data, base64Size, written);
    time_point<Clock> avx_end = Clock::now();
    nanoseconds avx_time = duration_cast<nanoseconds>(avx_end - avx_start);
    cout << "avx: " << avx_time.count() / (double)iterationCount << " ns" << endl;

    written = 0;
    time_point<Clock> sse_start = Clock::now();
    for (int k = 0; k < iterationCount; ++k)
        sse_decode(base64, data, base64Size, written);
    time_point<Clock> sse_end = Clock::now();
    nanoseconds sse_time = duration_cast<nanoseconds>(sse_end - sse_start);
    cout << "sse: " << sse_time.count() / (double)iterationCount << " ns" << endl;

    delete[] data;
    delete[] base64;
}
//-----------------------------------------------------------------------------
int main()
{
    for (int i = 0; i < 5; ++i)
    {
        run();

        cout << endl;
    }
}
