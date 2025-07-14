# Json.Transform Benchmarks

This project contains performance benchmarks for the Json.Transform library using BenchmarkDotNet.

## Running Benchmarks

To run the benchmarks, navigate to this directory and run:

```bash
cd benchmarks/Json.Transform.Benchmarks
dotnet run -c Release
```

**Note**: Always run benchmarks in Release mode for accurate performance measurements.

## Benchmark Categories

### 1. SimpleFieldMapping (Baseline)
Basic field mapping from source to destination paths.

### 2. ComplexTransformation
Multiple transformation operations including conditions, concatenation, and constants.

### 3. LargeDataAggregation
Aggregation operations on arrays with 100 items (sum, avg, count).

### 4. MathOperations
Mathematical calculations (power, add operations).

### 5. StringConcatenation
Template-based string concatenation with dynamic values.

### 6. ConditionalLogic
Nested conditional logic with if/else branches.

## Performance Targets

Based on the implementation goals:

- **Throughput**: 1000+ transformations/sec for medium JSON (~5KB)
- **Memory**: < 50MB for 1000 transformations
- **Latency**: < 10ms per transformation (P95)

## Sample Output

```
| Method                 | Mean      | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
|----------------------- |----------:|---------:|---------:|------:|-------:|----------:|------------:|
| SimpleFieldMapping     |  1.234 ms | 0.012 ms | 0.011 ms |  1.00 | 15.625 |     64 KB |        1.00 |
| ComplexTransformation  |  2.456 ms | 0.024 ms | 0.022 ms |  1.99 | 31.250 |    128 KB |        2.00 |
| LargeDataAggregation   |  5.678 ms | 0.056 ms | 0.053 ms |  4.60 | 62.500 |    256 KB |        4.00 |
| MathOperations         |  1.890 ms | 0.018 ms | 0.017 ms |  1.53 | 23.437 |     96 KB |        1.50 |
| StringConcatenation    |  1.567 ms | 0.015 ms | 0.014 ms |  1.27 | 19.531 |     80 KB |        1.25 |
| ConditionalLogic       |  1.789 ms | 0.017 ms | 0.016 ms |  1.45 | 21.875 |     90 KB |        1.41 |
```

## Understanding Results

- **Mean**: Average execution time
- **Error**: Standard error of the mean
- **StdDev**: Standard deviation
- **Ratio**: Performance relative to baseline
- **Gen0**: Number of Gen0 garbage collections per 1000 operations
- **Allocated**: Memory allocated per operation
- **Alloc Ratio**: Memory allocation relative to baseline

## Contributing

When adding new benchmarks:

1. Add methods with the `[Benchmark]` attribute
2. Ensure representative test data
3. Include relevant BenchmarkDotNet attributes (e.g., `[MemoryDiagnoser]`)
4. Update this README with new benchmark descriptions

## Tips for Accurate Benchmarks

1. Close unnecessary applications
2. Run on battery power (if laptop) to avoid thermal throttling
3. Use Release configuration
4. Run multiple iterations to account for variance
5. Consider using `[SimpleJob(RuntimeMoniker.Net90)]` for specific runtime targeting
