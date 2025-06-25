# ![Logo](.docs/Imperium.jpg) ntrospector

| Package | NuGet |
|---------|-------|
| Core |[![NuGet](https://img.shields.io/nuget/v/Introspector.svg)](https://www.nuget.org/packages/Introspector/)|
| Web |[![NuGet](https://img.shields.io/nuget/v/Introspector.WebApi.svg)](https://www.nuget.org/packages/Introspector.WebApi/)|
| Xml |[![NuGet](https://img.shields.io/nuget/v/Introspector.Xml.svg)](https://www.nuget.org/packages/Introspector.Xml/)|

## Show me what you got

Run `Introspector.Sample` service and open the address in your browser:

to see all use cases
```
http://localhost:3000/introspector/cases
```

to see sequence of the use case
```
http://localhost:3000/introspector/sequence?case={UseCaseName}

Examples:
http://localhost:3000/introspector/sequence?case=Use%20Case%201
```

to see components and their connections
```
http://localhost:3000/introspector/components?case={UseCaseName}

Examples:
http://localhost:3000/introspector/components
http://localhost:3000/introspector/components?case=Use%20Case%201
```

to see all available schemas in one output
```
http://localhost:3000/introspector/all
```

Then you can use [PlantUml Service](https://editor.plantuml.com/) to draw the output diagrams.

## Have fun!