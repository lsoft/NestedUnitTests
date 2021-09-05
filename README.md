# Nested Unit Tests Visual Studio Extension

What's about to move your unit tests closer to the code? This project proposes to check whether life would be easier if the unit tests lay next to the code they are testing?

Of course, this approach will bring new difficulties. For example: you will need to exclude that tests from your production-ready binaries. Fair price for your convenience, isn't?

## How to try

- Open your Visual Studio 2019 and waits for `NestedUnitTests` Vsix finished its loading.
- Create a new C# class library.
- **If you are using SDK style project**: click RMB on project node in Solution Explorer and choose [Prepare project for nested unit tests](1.png)
- Click RMB on C# file node in Solution Explorer and choose [Add unit tests](2.png)
- New C# file with the unit test can now be opened under the [file you choosed](3.png).

[You can also set this Vsix up](4.png)

To exclude you tests from compilation for production use the following construction `dotnet build /p:DefineConstants=SKIP_NESTED_TESTS`.


This is a very early version of the vsix! You can download it for Visual Studio 2019 [here](https://marketplace.visualstudio.com/items?itemName=lsoft.NestedUnitTests).

## Screenshots

![Prepare project for nested unit tests](1.png)


![Add unit tests](2.png)


![Result you will have](3.png)


![You can also set this Vsix up](4.png)
