```

BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6456/22H2/2022Update)
Intel Core i7-8700T CPU 2.40GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK 10.0.102
  [Host]     : .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 9.0.12 (9.0.12, 9.0.1225.60609), X64 RyuJIT x86-64-v3


```
| Method                                                | ConcurrentTasks | Mean      | Error    | StdDev   | Median    | Gen0      | Gen1      | Gen2     | Allocated  |
|------------------------------------------------------ |---------------- |----------:|---------:|---------:|----------:|----------:|----------:|---------:|-----------:|
| **ExecuteAsync_MultipleConcurrentTasks_NoCancellation**   | **1**               | **109.97 ms** | **0.551 ms** | **0.515 ms** | **109.98 ms** |         **-** |         **-** |        **-** |      **992 B** |
| ExecuteAsync_MultipleConcurrentTasks_WithCancellation | 1               |  15.79 ms | 0.047 ms | 0.042 ms |  15.81 ms |         - |         - |        - |     3232 B |
| Cancel_Performance_UnderLoad                          | 1               |  15.78 ms | 0.070 ms | 0.066 ms |  15.79 ms |         - |         - |        - |     3080 B |
| **ExecuteAsync_MultipleConcurrentTasks_NoCancellation**   | **10**              | **110.45 ms** | **0.269 ms** | **0.252 ms** | **110.51 ms** |         **-** |         **-** |        **-** |     **7544 B** |
| ExecuteAsync_MultipleConcurrentTasks_WithCancellation | 10              |  15.77 ms | 0.056 ms | 0.049 ms |  15.78 ms |         - |         - |        - |    23606 B |
| Cancel_Performance_UnderLoad                          | 10              |  15.77 ms | 0.058 ms | 0.051 ms |  15.79 ms |         - |         - |        - |    22585 B |
| **ExecuteAsync_MultipleConcurrentTasks_NoCancellation**   | **100**             | **110.30 ms** | **0.333 ms** | **0.296 ms** | **110.30 ms** |         **-** |         **-** |        **-** |    **73064 B** |
| ExecuteAsync_MultipleConcurrentTasks_WithCancellation | 100             |  15.78 ms | 0.066 ms | 0.059 ms |  15.80 ms |   31.2500 |         - |        - |   225608 B |
| Cancel_Performance_UnderLoad                          | 100             |  15.78 ms | 0.061 ms | 0.057 ms |  15.80 ms |   31.2500 |         - |        - |   215966 B |
| **ExecuteAsync_MultipleConcurrentTasks_NoCancellation**   | **1000**            | **110.29 ms** | **0.494 ms** | **0.462 ms** | **110.32 ms** |         **-** |         **-** |        **-** |   **728264 B** |
| ExecuteAsync_MultipleConcurrentTasks_WithCancellation | 1000            |  16.11 ms | 0.318 ms | 0.312 ms |  16.17 ms |  343.7500 |  250.0000 |        - |  2241627 B |
| Cancel_Performance_UnderLoad                          | 1000            |  15.98 ms | 0.302 ms | 0.282 ms |  15.82 ms |  343.7500 |  281.2500 |        - |  2145552 B |
| **ExecuteAsync_MultipleConcurrentTasks_NoCancellation**   | **10000**           | **119.45 ms** | **1.217 ms** | **1.139 ms** | **119.26 ms** | **1000.0000** |  **750.0000** |        **-** |  **7280264 B** |
| ExecuteAsync_MultipleConcurrentTasks_WithCancellation | 10000           |  90.56 ms | 1.723 ms | 1.984 ms |  91.06 ms | 4500.0000 | 2666.6667 | 833.3333 | 22516884 B |
| Cancel_Performance_UnderLoad                          | 10000           | 102.64 ms | 2.226 ms | 6.563 ms | 104.55 ms | 4333.3333 | 2500.0000 | 833.3333 | 21571451 B |
