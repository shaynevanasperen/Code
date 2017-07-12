<img src="Code.png" align="right" />

[![Build status](https://ci.appveyor.com/api/projects/status/0a3xgtsq2qgnml8d?svg=true)](https://ci.appveyor.com/project/shaynevanasperen/code)
[![Join the chat at https://gitter.im/code-packages](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/code-packages)
![License](https://img.shields.io/github/license/shaynevanasperen/code.svg)

## Code

A collection of source-only [NuGet packages](https://www.nuget.org/packages?q=code) containing
tiny bits of utility code that don't belong together in a monolithic library. These are
typically delivered as singular source files added to your project as internal classes that
can easily be kept up-to-date with NuGet.

### Why

Who doesn't have an ever growing and ever less cohesive miscellaneous collection of helpers,
extension methods and utility classes in the usual "[Common.dll](http://ayende.com/blog/3986/let-us-burn-all-those-pesky-util-common-libraries)"?
Well, the problem is that there's really no good place for all that baggage: do we split them by
actual behavioral area and create "proper" projects for them?

In most cases, that's totally overkill and you end up in short time with the same pile of assorted
files as you try to avoid setting up an entire new project to contain just a couple cohesive classes.
You also risk running into 
[the dreaded diamond dependency problem](https://www.well-typed.com/blog/2008/04/the-dreaded-diamond-dependency-problem/),
as the more popular your library becomes, the more chance there is that different versions of it
will already be referenced by other libraries you want to consume.

It turns out that in the vast majority of cases, those helpers are just meant for internal
consumption by the actual important parts of your code. A relatively underused feature of NuGet is the
ability to deliver source code in a package, and it happens to be the perfect fit for this type
of code. So this is a repository of the source and accompanying unit tests for all those
helpers, neatly organized by target namespace being extended, deployed using NuGet and licensed
under MIT for everyone to use and contribute.

### Conventions

It's worth taking note of a few conventions that make publishing and consuming _Code_ packages
a pleasant and rewarding experience:
* Types and members are declared with the `internal` access-modifier so that consuming projects
don't unintentionally expose them as part of their public API.
* Types are marked as `partial` so that consuming projects can extend them more easily and so that
multiple packages can declare members in the same type.
* Packages are marked as [development dependencies](https://docs.nuget.org/release-notes/nuget-2.8#development-dependencies)
so that they don't become transitive for consuming projects.
* Packages are intentionally very fine-grained, typically containing only a single method.
* Packages can depend on other source code packages.
* Dependencies on other compiled libraries are intentionally omitted so that consuming projects
can use whichever version they want as long as the code still compiles.

### Modifying the code delivered in a source code package

In `.csproj` scenarios, installing a source code package results in the code being compiled with your
project, but it doesn't get copied into your project folder. Instead, it remains in your local
NuGet package cache folder where it won't be checked into source control and so editing is not possible.

In `packages.config` scenarios, installing a source code package results in the source code being
copied into your project folder. From then on, that code will be part of your project and should be
checked into source control. If you modify that code and then try to update the package to a newer
version you will be prompted for whether or not to overwrite it with the newer version from the package.
If you choose to overwrite it, then provided it was under source control you can merge your
customization(s) back in, or choose to accept the newer version from the package. If you never modify
the code then it will be silently overwritten when updating the package.

### Acknowledgements

The inspiration for creating this collection of packages was born out of my own frustrations related
to bundling miscellaneous helpers into a [monolithic library](http://ayende.com/blog/3986/let-us-burn-all-those-pesky-util-common-libraries),
seeing it done before in [NETFx](https://netfx.codeplex.com/), and from reading a blog post titled
[Packaging Source Code With NuGet](http://nikcodes.com/2013/10/23/packaging-source-code-with-nuget/).
I decided to create this project instead of contribute to NETFx because I was frustrated with how
cumbersome it is to contribute new packages to NETFx, as well as the fact that NETFx packages are
not marked as [development dependencies](https://docs.nuget.org/release-notes/nuget-2.8#development-dependencies).