// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Text;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);