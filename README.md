BONSAI is an open-source visual programming language for data stream processing built on top of [Rx.NET](http://reactivex.io/). With BONSAI you tell your computer what to do not through long listings of text but by manipulating graphical elements in a workflow. Like in Rx, workflow elements in BONSAI represent asynchronous streams of data which can be connected together to perform complex operations.

A quick intro
-------------

The main goal of BONSAI is to make it easier to write programs that combine the acquisition and processing of many heterogeneous streams of data. These data streams can come from a variety of devices, including cameras, microphones, embedded microcontrollers, the network or files in your hard drive. BONSAI includes a variety of packages that take care of the tedious work of setting up and extracting data from all these devices.

Included in BONSAI are various algorithms for image and digital signal processing that allow you to extract information from these raw data streams. Parameters can be manipulated and altered online using property pages and each processing step can be independently visualized while the workflow is running. It is also possible to change these parameters dynamically by using the output of other elements.

Finally, BONSAI also includes several modules that allow you to specify useful side effects you might want to achieve with the processed data, such as saving it to a file, actuating a servomotor, or playing sound through the speakers. It is even possible to organize the message passing logic of BONSAI to design reactive asynchronous state machines to implement control procedures.

Under the hood
--------------

BONSAI is developed entirely in C# and one of its main features is extensibility and interoperability with the rest of the .NET framework and other Rx applications. In fact, every BONSAI module is just a standard C# class exposing [Observable](http://reactivex.io/documentation/observable.html) methods. This means that you can reference every single BONSAI package in your standard .NET application and just call the code as you would with any other library. There is no extra runtime.

In its visual environment, BONSAI makes use of [attribute metadata](http://msdn.microsoft.com/en-us/library/5x6cd29c.aspx), type inference and [expression trees](http://msdn.microsoft.com/en-us/library/bb397951.aspx) to automatically generate [MSIL](http://msdn.microsoft.com/en-us/library/c5tkafs1.aspx) code that implements your design. This has the consequence that running a BONSAI workflow is as fast as if you wrote the code yourself!

It is very easy to extend BONSAI with your own modules. There is no need to learn yet another API as BONSAI uses entirely Rx under the hood. If you are familiar with programming using [Rx](http://reactivex.io/) or [LINQ](http://msdn.microsoft.com/en-us/library/hh242983.aspx) you're good to go. After you've created your package just upload it to the [NuGet](http://www.nuget.org/) gallery and you will be sharing it with the whole BONSAI community using the integrated package manager.