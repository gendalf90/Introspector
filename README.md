# ![Logo](.docs/Imperium.jpg) ntrospector

| NuGet |
|-------|
|[![NuGet](https://img.shields.io/nuget/v/Introspector.svg)](https://www.nuget.org/packages/Introspector/)|

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

Then you can use [PlantUml Service](https://editor.plantuml.com/) to draw the output diagrams.

## Have fun!