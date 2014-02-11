# SolutionChecker

Utility to detect potential issues in Visual Studio solutions, through multiple checkers, each with their specialties :

1. NuGet package reference checker
2. Static assembly reference checker
3. Project version checker

## NuGet package reference checker

Scope: solution.
Emits errors.

Lists NuGet package references used by VC# projects, and checks that packages referenced by all projects do not come in multiple versions, hence referencing multiple different assemblies.

## Static assembly reference checker

Scope: project.
Emits either errors or warnings.

Checks that project reference links exist and are at the referenced versions.

## Project version checker

Scope: project
Emits warnings.

Checks that versions within a project are coherent.