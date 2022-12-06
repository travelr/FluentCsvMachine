![example workflow](https://github.com/travelr/readerFlu/actions/workflows/ci.yml/badge.svg)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Fluent.CSV.Machine)](https://www.nuget.org/packages/Fluent.CSV.Machine/)
![GitHub](https://img.shields.io/github/license/travelr/FluentCsvMachine)
![.NET Version](https://img.shields.io/badge/requires%20.NET-7.0-green)

# Fluent CSV Machine

> ## Features

- Reads and parses each character only once.  
-> results in a blazing fast execution while having a low memory footprint  
<sub>(does **not** follow the usual pattern: read the line, split it, parse the fields)</sub>  
- Supports all [CSV](https://en.wikipedia.org/wiki/Comma-separated_values#Basic_rules) variants, test cases are implemented against those
- Types are parsed directly. All types support also [nullable](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types) except Enums
	- Simple types (int, long, double, decimal, ...)
	- string
	- DateTime -> requires an specific [InputFormat](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)
	- Enums
- Fluent interfaces to define the mapping between the Entity class and the CSV file
- Parsing and entity creation is handled by different threads

> ## Getting started

Please take a look at the [documentation](https://travelr.github.io/FluentCsvMachine/api/index.html)
as well at those [implemented test cases](https://github.com/travelr/FluentCsvMachine/blob/main/test/CsvWithHeader.cs).  
The test cases implement a variety of different CSV fixtures (which are mainly forked from from [csv-parser](https://github.com/mafintosh/csv-parser))

```C#
// CSV file content:
// a,b,c
// 1,2,2012/11/25
// 3,4,2022/12/04

var parser = new CsvParser<EntityClass>();
parser.Property<string?>(c => c.A).ColumnName("a");
parser.Property<int>(c => c.B).ColumnName("b");
parser.Property<DateTime>(c => c.C).ColumnName("c").InputFormat("yyyy/MM/dd");
IReadOnlyList<EntityClass> result = await parser.Parse(path);

// Values are parsed according to their type definition in EntityClass
```

*Hint:*  
Have a look at this awesome [tool](https://toolslick.com/generation/code/class-from-csv) which generates Entity classes. This tool belongs to the popular library [CSVHelper](https://github.com/JoshClose/CsvHelper).

> ## Benchmark

Parsing 13 columns, the CSV file contains 14.  
Data is read from a MemoryStream and returned as List of [Entities](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs) with the default parser [configuration](https://github.com/travelr/FluentCsvMachine/blob/main/library/CsvConfiguration.cs).  
Each [Entity](https://github.com/travelr/FluentCsvMachine/blob/main/test/Models/BigDataSet.cs) contains 7 string, 2 int, 2 double, 1 DateTime and 1 Enum Property.  
The benchmark ranges from 1 thousand to 1 million CSV lines / entities.  


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


[Here](https://www.joelverhagen.com/blog/2020/12/fastest-net-csv-parsers) you can compare those values roughly. Though different libraries have different purposes while all parse CSV.

> ## Background

This started as CSV library for my personal private projects. 
My thought back then was the following: Do not test a dozen libraries, just write one of your one.
Since then it has been rewritten a few times. Mostly to show off that I can still write effient code while my occupation doesn't include any programming anymore.
Finally I tried to make it as fast as possible while still returning a typed result and not just a set of strings. 

tl;dr: Lets see how fast a typed dotnet CSV parser can get

> ## Advanced use cases
> ### Custom Properties

If a simple mapping does not work out for you then you can try to use [PropertyCustom](https://travelr.github.io/FluentCsvMachine/api/FluentCsvMachine.Property.CsvPropertyCustom-2.html)

```C#
parser.PropertyCustom<string>((x, v) =>
{
    var split = v.Split(' ').Select(c => c.Trim()).ToArray();
    x.ForeignCurrencyValue = decimal.Parse(split[0]);
    x.Currency = EnumHelper.Parse<Currency>(split[1]);
}).ColumnName("Foreign Transaction");
```

Beware: You are about to execute this [Action](https://learn.microsoft.com/en-us/dotnet/api/system.action?view=net-7.0) on each CSV line.
An Action which is a lot slower than the in-built parsers.

> ### Line Actions

Defines one or more [Actions](https://learn.microsoft.com/en-us/dotnet/api/system.action?view=net-7.0)
which run after all properties (normal as well as custom ones) have been mapped.

```C#
var parser = new CsvParser<T>();
parser.LineAction((obj, fields) =>
{
	if (fields == null || obj is not Entity e)
	{
		return;
	}
	// Create an hash value using all parsed columns of a CSV line
	e.HashCode = HashCodeLine(fields);
});
```

Have a look at the test case [BackTick](https://github.com/travelr/FluentCsvMachine/blob/d2128dd90c2938f185a8179112e027ae1814f716/test/CsvWithHeader.cs#L74) for another example

> ### CSV files without a header line 

This CSV parser only works with a backing class which can be mapped. If you do not have a CSV line which defines the headers and thefore the corresponding properties:   
Then you need to use [CsvNoHeaderAttribute](https://travelr.github.io/FluentCsvMachine/api/FluentCsvMachine.CsvNoHeaderAttribute.html)
as an attribute to your properties to define the column order.

```C#
internal class BasicString
{
    [CsvNoHeader(columnIndex: 0)] public string? A { get; set; }
    [CsvNoHeader(columnIndex: 1)] public string? B { get; set; }
    public string? C { get; set; } // This column won't be mapped
    [CsvNoHeader(columnIndex: 2)] public string? D { get; set; }
}
```


