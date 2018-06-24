# FileArchiver
Simple file archive/copy service written in .NET Core 2.1. Currently, the main use case for this app is for me to easily transfer photos from my camera cards to my image repo on disk storage. Because I have a very specific use case and directory structure that I use for storing images, I have not spent a lot of time customizing this app.

# Features

* Copies all files in source directory/subdirectories to destination directory.
* Destination files will be copied using the pattern ROOT\yyyy-DD-mm\yyyyMMdd-HHmmss_<Original Filename>
* Uses Task Parallel Library (TPL) with -p argument to make better use of multi-core systems.

# Future Enhancements

* Provide customizable destination subdirectory structure.

# Third-Party Nuget Dependencies

* [McMaster.Extensions.CommandLineUtils](https://github.com/natemcmaster/CommandLineUtils) - Fork of discontinued .NET libary to handle command-line args.
