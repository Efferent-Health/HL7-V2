# HL7-V2

[![NuGet](https://img.shields.io/nuget/v/HL7-v2.svg)](https://www.nuget.org/packages/HL7-V2/)
![downloads](https://img.shields.io/nuget/dt/HL7-v2)
![github](https://img.shields.io/github/stars/Efferent-Health/HL7-v2?style=flat&color=yellow)
![build](https://github.com/Efferent-Health/HL7-v2/actions/workflows/main.yml/badge.svg?branch=main)

This is a lightweight library for composing and parsing HL7 2.x messages, for modern .NET applications. 

It is not tied to any particular version of HL7 nor validates against one. 

## Usage and compatibility

This library is distributed via [nuget](https://www.nuget.org/packages/HL7-v2/latest) and targets two frameworks:
- .NET Standard 2.0 for maximum compability, covering more than 40 .NET frameworks
- .NET 8.0 for better performance under the new Microsoft's cross-platform framework

For using the classes and methods mentioned below, declare de following namespace:

````cs
using Efferent.HL7.V2;
````

## Documentation

Read the full documentation in the [wiki page](https://github.com/Efferent-Health/HL7-V2/wiki/Documentation#documentation).

## Credits
This library has taken Jayant Singh's HL7 parser as its foundation: https://github.com/j4jayant/hl7-cSharp-parser

The field encoding and decoding methods have been based on: https://github.com/elomagic/hl7inspector
