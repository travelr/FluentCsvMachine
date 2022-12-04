![example workflow](https://github.com/travelr/readerFlu/actions/workflows/ci.yml/badge.svg)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Fluent.CSV.Machine)](https://www.nuget.org/packages/Fluent.CSV.Machine/)
![GitHub](https://img.shields.io/github/license/travelr/FluentCsvMachine)

# Fluent CSV Machine

> ### Features

An expressions based CSV parser  
Reads each character only once
...

> ### Getting started

Please take a look at the documentation as well as the [implemented test cases](https://github.com/travelr/FluentCsvMachine/blob/main/test/CsvWithHeader.cs).
The test cases implement a variety of different CSV fixtures, why are mainly forked from from [csv-parser](https://github.com/mafintosh/csv-parser) 

	// CSV file:
	// a,b,c
	// 1,2,3

	var parser = new CsvParser<Basic>();
	parser.Property<string>(c => c.A).ColumnName("a");
	parser.Property<int>(c => c.B).ColumnName("b");
	parser.Property<decimal?>(c => c.C).ColumnName("c");
	var result = await parser.Parse(path);

> ### Benchmark

Parsing 13 columns, CSV contains 14. Data is read from a MemoryStream and returned as List of [Entities](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs).  
Each [Entity](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs) contains 7 string, 2 int, 2 double, 1 DateTime and 1 Enum Property.  
The benchmark ranges from 1 thousand to 1 millions CSV lines / entities. 


	BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.1098/21H2)
	AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
	.NET SDK=7.0.100
	  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
	  Job-INKOXJ : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

	InvocationCount=1  IterationCount=10  LaunchCount=10
	RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=0

	|    Method |        Lines |         Mean |      Error |      StdDev |       Median |
	|---------- |------------- |-------------:|-----------:|------------:|-------------:|
	| Benchmark |        1,000 |     6.789 ms |  0.1206 ms |   0.3556 ms |     6.667 ms |
	| Benchmark |       10,000 |    34.383 ms |  4.3588 ms |  12.8519 ms |    44.196 ms |
	| Benchmark |      100,000 |   240.696 ms |  1.4216 ms |   4.1915 ms |   239.989 ms |
	| Benchmark |    1,000,000 | 2,742.841 ms | 47.3035 ms | 139.4755 ms | 2,815.532 ms |

> ### CSV custom actions

asdas

> ### CSV files without header

...


