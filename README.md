# ![Logo](.docs/Imperium.jpg) ntrospector

| NuGet |
|-------|
|[![NuGet](https://img.shields.io/nuget/v/Introspector.svg)](https://www.nuget.org/packages/Introspector/)|

## Show me what you got

Run `Introspector.Sample` service and open the address in your browser:

to see all use cases
```
http://localhost:51000/introspector/cases
```

to see sequence of the use case
```
http://localhost:51000/introspector/sequence?case={UseCaseName}&scale={UseCaseScale}

Examples:
http://localhost:51000/introspector/sequence?case=use%20case%201
http://localhost:51000/introspector/sequence?case=use%20case%201&scale=2
```

to see components and their connections
```
http://localhost:51000/introspector/components?case={UseCaseName}&scale={ComponentsScale}

Examples:
http://localhost:51000/introspector/components
http://localhost:51000/introspector/components?case=use%20case%201
http://localhost:51000/introspector/components?scale=2
```

Then you can use [PlantUml Service](https://www.plantuml.com/) to draw the output diagrams.

## Have fun!