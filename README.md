![example workflow](https://github.com/travelr/readerFlu/actions/workflows/ci.yml/badge.svg)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Fluent.CSV.Machine)](https://www.nuget.org/packages/Fluent.CSV.Machine/)
![GitHub](https://img.shields.io/github/license/travelr/FluentCsvMachine)

**Documentation not finished yet**

# Fluent CSV Machine

> ### Features

- Reads and parses each character only once.  
*(does not follow the usual pattern: read the line, split it, parse the fields)*  
-> results in a speedy execution while having a low memory footprint
- Supports all [CSV](https://en.wikipedia.org/wiki/Comma-separated_values#Basic_rules) variants, test cases are implemented against those
- Types are parsed directly. All types except Enum also support [nullable](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types) 
	- Simple types (int, long, double, decimal, ...)
	- string
	- DateTime -> requires an specific [InputFormat](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)
	- Enums
- Fluent interfaces to define the mapping between the Entity class and the CSV file
- Parsing the CSV and enitity creation is handeled by different threads
- The EntityFactory is using [Linq.Expressions](https://learn.microsoft.com/en-us/dotnet/api/system.linq.expressions?view=net-7.0) to do its job

> ### Getting started

Please take a look at the documentation as well as the [implemented test cases](https://github.com/travelr/FluentCsvMachine/blob/main/test/CsvWithHeader.cs).  
The test cases implement a variety of different CSV fixtures, which are mainly forked from from [csv-parser](https://github.com/mafintosh/csv-parser) 

	// CSV file:
	// a,b,c
	// 1,2,2012/11/25
	// 3,4,2022/12/04

	var parser = new CsvParser<EntityClass>();
	parser.Property<string?>(c => c.A).ColumnName("a");
	parser.Property<int>(c => c.B).ColumnName("b");
	parser.Property<DateTime>(c => c.C).ColumnName("c").InputFormat("yyyy/MM/dd");
	IReadOnlyList<EntityClass> result = await parser.Parse(path);

	// Values are parsed according to their type definition in EntityClass

*Hint:* Have a look at this awesome [tool](https://toolslick.com/generation/code/class-from-csv) to generate Entity classes. This tool belongs to the popular library [CSVHelper](https://github.com/JoshClose/CsvHelper)

> ### Benchmark

Parsing 13 columns, CSV contains 14. Data is read from a MemoryStream and returned as List of [Entities](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs) with the default [configuration](https://github.com/travelr/FluentCsvMachine/blob/main/library/CsvConfiguration.cs).  
Each [Entity](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs) contains 7 string, 2 int, 2 double, 1 DateTime and 1 Enum Property.  
The benchmark ranges from 1 thousand to 1 millions CSV lines / entities.  


	BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22000.1098/21H2)
	AMD Ryzen 5 3600, 1 CPU, 12 logical and 6 physical cores
	.NET SDK=7.0.100

	InvocationCount=1  IterationCount=10  LaunchCount=10
	RunStrategy=Monitoring  UnrollFactor=1  WarmupCount=0

	|        Lines |         Mean |      Error |      StdDev |       Median |
	|------------- |-------------:|-----------:|------------:|-------------:|
	|        1,000 |     6.789 ms |  0.1206 ms |   0.3556 ms |     6.667 ms |
	|       10,000 |    34.383 ms |  4.3588 ms |  12.8519 ms |    44.196 ms |
	|      100,000 |   240.696 ms |  1.4216 ms |   4.1915 ms |   239.989 ms |
	|    1,000,000 | 2,742.841 ms | 47.3035 ms | 139.4755 ms | 2,815.532 ms |

> ### Background

This started as CSV library for my personal private projects. 
My thought back then was the following: Do not test a dozen libraries, just write one of your one.
Since then it has been rewriten a few times. Mostly to show off that I can still write effient code while my current jobs doesn't include any programming anymore.
Finally I tried to make it as fast as posible while still returning a typed result and not just a set of strings. 

tl;dr: Lets see how fast a typed CSV parser can get

> ### CSV custom actions

...

> ### CSV files without header

...


