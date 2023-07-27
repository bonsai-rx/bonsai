# Bonsai - Visual Reactive Programming

This is the main repository for the [Bonsai](https://bonsai-rx.org/) visual programming language. It contains source code for the compiler, IDE, and standard library.

With Bonsai you tell your computer what to do not through long listings of text but by manipulating graphical elements in a workflow. Bonsai is built on top of [Rx.NET](http://reactivex.io/), and like in Rx, workflow elements in Bonsai represent asynchronous streams of data called [Observables](https://bonsai-rx.org/docs/articles/observables.html) which can be connected together to perform complex operations.

Building from Source
--------------------

### Windows

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/). The Community Edition is available free of charge for open-source projects. Make sure the **.NET Desktop Development** workload is selected when running the installer.
2. Open `Bonsai.sln` and select `Build` > `Build Solution` from the VS menu.

### Installer

1. Install the [Wix Toolset build tools](https://wixtoolset.org/releases/) version 3.11 or greater.
2. From Visual Studio menu, select `Extensions` > `Manage Extensions` and install the WiX Toolset Visual Studio 2022 Extension.

### Debugging

The new bootstrapper logic makes use of isolated child processes to manage local editor extensions. To make it easier to debug the entire process tree we recommend installing the [Child Process Debugging Power Tool](https://devblogs.microsoft.com/devops/introducing-the-child-process-debugging-power-tool/) extension.

Getting Help
------------

You can find the Bonsai community in a few places:
 * [GitHub](https://github.com/bonsai-rx/bonsai/discussions) - Announcements, general discussion and Q&A
 * [Discord](https://discord.gg/K8jUKH7) - General discussion

Contributing
------------

Bonsai is a programming language with a growing community of users and developers. Anyone is welcome to participate by reporting bugs, proposing features or improvements, or contributing pull requests directly to our open-source repositories.

For more information please see our [Contributor Guide](https://bonsai-rx.org/contribute/). By contributing to our project, we also expect you to uphold our community [Code of Conduct](https://bonsai-rx.org/code-of-conduct).

Documentation
-------------

The Bonsai [documentation](https://bonsai-rx.org/docs/) is open to community contributions. If you are interested in helping us to improve it, please take a look at our [docs repo](https://github.com/bonsai-rx/docs).
