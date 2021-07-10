# Disclaimer

This project is created using the Windows Presentation Foundation and as such is only available for Windows. If you attempt to build or run this project on a different operating system, you will get an error and be unable to continue.

# How to Build and Run the Application

Before being able to build the application, you first need to clone the repository. This can easily be done by clicking the blue "Clone" button at the top of this page. You can either use SSH, HTTPS or copy right to your IDE (only Visual Studio seems to be supported by GitLab). It's up to your preference which one you select.

For building the application the .Net Core 3.1 SDK is required. This can be found here: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). Now that you have the project cloned and the SDK installed you can build the application. You can either use your IDE for doing so or you can build it using the Command Line. We will explain how it can be done from the CL. You first need to open a terminal in the folder where you have cloned the repository or navigate to that folder from a terminal. Now all you have to do to build the project is run one of the following commands:

```powershell
dotnet build
dotnet build --configuration Release
```

More information about this command and its parameters can be found here: [https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build)

The first command will build the application in Debug mode while the second one will build it in Release mode. The first command should be used when developing the project further while the second command is preferred for deploying the software (as this gives increased performance). Both of these commands create executable files that are stored in the "HandwritingFeedback\bin" folder.

After following these steps, you can run the application by navigating to the aforementioned folder. There you will find an executable that can be double-clicked to run the application.

# Running Tests

To run the unit tests we have created for this project, you will need the .Net Core 3.1 SDK mentioned above. To run the tests using the Command Line, you will need to open a terminal in the folder where the repository was cloned. Here, you will need to run the following command:

```powershell
dotnet test
```

More information about this command and its parameters can be found here: [https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test)

# Additional Resources
<Add here links to the readme folder>

[Software Architecture](readme/software-architecture.md)

[Adding New Feedback Types and Input Sources](readme/adding-feedback-types-and-input-sources.md)

