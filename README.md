---
page_type: sample
languages:
- csharp
- aspx-csharp
- javascript
- html
products:
- aspnet-core
- dotnet-core
- azure-iot-edge
- vs-code
description:
- Sample HMI module for deployment on Azure IoT Edge which displays generated data in an ASP.NET Core WebApp HTML page.
---

# Azure IoT Edge HMI Module to Display Data on Device

This repo contains an Azure IoT Edge HMI module demonstrating void detection using C#.

It includes an Azure IoT Edge deployment solution for a single module with a route that can receive data from the IoT 
EdgeHub and a data generator for demonstration purposes. It also contains a ASP.NET Core WebApp with a grocery store 
shelf image that updates dynamically based on data from either source.
Given the amount of updates necessary throughout the module code for a different scenario/data, the generator is an 
extremely useful tool for testing/updating purpoes.

Details on the specifics of the WebApp module, including how to turn the generator on/off, are included in the 
[WebApp Readme](./modules/WebApp/Readme.md)

The sample code is configured to build and run in [Visual Studio Code](https://code.visualstudio.com/) (aka VSCode). 
Visual Studio Code is a Microsoft cross platform code editor, which also includes powerful developer tooling, like 
IntelliSense code completion and debugging. Visual Studio Code is available on Windows, Mac, and Linux and supports C# 
and Node debugging for Azure IoT Edge modules.

## Contents

| File/folder | Description |
|-|-|
| `.vscode`  | Visual Studio Code files to debug the WebApp |
| `modules` | Folder containing 1+ IoT Edge Modules, WebApp only for this sample |
| `.gitignore`      | Define what to ignore at commit time. |
| `CODE_OF_CONDUCT.md` | Microsoft Open Source Code of Conduct and FAQ |
| `CONTRIBUTING.MD` | Guidelines for contributing to the Sample on Github |
| `EdgeDevelopment.md` | Azure IoT Edge development environment configuration |
| `ISSUE_TEMPLATE.md`  | Template for submitting issues on GitHub |
| `LICENSE`         | The license for the sample |
| `README.md`       | This README file |
| `SECURITY.md`     | Instructions for filing security issues directly with Microsoft |

## Prerequisites

### Minimum Sample Run

The quickest and most minimal way to run the sample work is to install

- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET Core SDK (2.1 or above)](https://dotnet.microsoft.com/download) - only required for C# version of sample

and then run the sample in debug mode (F5). This is only possible because this module is self-contained with an 
internal data generator. Go to http://localhost:8080 and you will see the data displaying on the webpage.

To learn the [key concepts](#key-concepts) or to run the module in the VS Code IoT Edge simulator as you would another 
module, please follow the guidelines below.

### Development machine prerequisites

The sample is designed to be built and run on a development machine in Visual Studio Code using Docker CE and the Azure 
IoT EdgeHub Dev Tool. Below are the prerequisites to build and run the sample on a local development machine: 

1. Language SDK's
    - [.NET Core SDK (2.1 or above)](https://dotnet.microsoft.com/download)
    - [Python (2.7/3.6 or above) and Pip](https://www.python.org/) - required for Azure IoT EdgeHub Dev Tool. 
    **Windows users should select the option to add Python to the path.**

2. Docker

    [Docker Community Edition](https://docs.docker.com/install/) - required for Azure IoT Edge module development, 
    deployment and debugging. Docker CE is free, but may require registration with Docker account to download. Docker 
    on Windows requires Hyper-V support. Please make sure your Windows version supports Hyper-V. For Windows 10, 
    Hyper-V is available with the Pro or Enterprise versions.

3. Visual Studio Code and extensions

    > **Note**: Extensions can be installed either via links to the Visual Studio Code Marketplace below or by searching 
    extensions by name in the Marketplace from the Extensions tab in Visual Studio Code.

    Install [Visual Studio Code](https://code.visualstudio.com/) first and then add the following extensions:

    - [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp) - provides C# syntax 
    checking, build and debug support
    - [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools) - provides 
    Azure IoT Edge development tooling

        > **Note**: Azure IoT Tools is an extension pack that installs 3 extensions that will show up in the Extensions 
        pane in Visual Studio Code - *Azure IoT Hub Toolkit*, *Azure IoT Edge* and *Azure IoT Workbench*.

4. Azure IoT EdgeHub Dev Tool

    [Azure IoT EdgeHub Dev Tool](https://pypi.org/project/iotedgehubdev/) is a version of the Azure IoT Edge runtime for 
    local development machine. After verifying Python and Pip (2.7/3.6 or above) are installed and in the path, install 
    **[iotedgehubdev](https://pypi.org/project/iotedgehubdev/)** with Pip:

    ```bash
    pip install --upgrade iotedgehubdev
    ```

### Azure Prerequisites

1. Azure IoT Hub Service

    To run the samples, you will need an Azure subscription and a provisioned Azure IoT Hub service.. Every Azure 
    subscription allows one free F1 tier Azure IoT Hub. The F1 tier Azure IoT is sufficient for this sample. 

    [Create an IoT hub using the Azure portal](https://docs.microsoft.com/en-us/azure/iot-hub/quickstart-send-telemetry-dotnet#create-an-iot-hub)

2. Azure Container Registry (optional)

    This sample can be built and run in the local Azure IoT Edge Simulator without pushing Azure IoT Edge modules to a 
    container registry. A container registry is only needed when deploying to an actual Azure IoT Edge device. Any 
    Docker container registry can be used, including DockerHub and Azure Container Registry.

    [Create an Azure Container Registry](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal)

## Setup

### Configure Azure IoT Edge development environment

1. Connect your Azure account to Visual Studio Code

   The *Azure IoT Tools* Visual Studio Code extension pack installs a prerequisite *Azure Account* extension if its not 
   already present. This extension allows Visual Studio Code to connect to your Azure subscription. For this sample, 
   Visual Studio Code needs to connect to you Azure IoT Hub service.

   Open the command palette and search for *Azure: Sign In*

   Select this command and you will be prompted to sign into your Azure account in a separate browser window. After 
   sign-in, you should see *Azure:* followed by your login account in the status bar at the bottom of Visual Studio 
   Code.

2. Connect to your Azure IoT Hub

   There are 2 ways to connect to your Azure IoT Hub from within Visual Studio Code:

   Open the command palette and search for *Azure IoT Hub: Select IoT Hub* 

   ​	**or**

   With the Explorer icon in the Visual Studio Code Activity Bar selected, go to the *AZURE IOT HUB* section in the 
   Explorer pane of Visual Studio Code. Select the "..." to open the Azure IoT Hub context menu. From the Context 
   Menu, choose *Select IoT Hub*. 

   Both options will open a selection list of available subscriptions at the top of the Visual Studio window. After 
   selecting your subscription, all available Azure IoT Hubs in your subscription will be presented in another selection 
   list. After selecting your Azure IoT Hub, the *AZURE IOT HUB** section in the Explorer pane of Visual Studio Code 
   will be populated with configured Devices and Endpoints. The Devices list will initially be empty for a new Azure 
   IoT Hub.

3. Create an Azure IoT Edge device 

   This sample is designed to run in the Azure IoT Edge Simulator on a local development machine. However, the 
   Simulator still connects to your Azure IoT Hub service, and therefore needs an Azure IoT Edge device definition in 
   Azure IoT Hub. You can create an Azure IoT Edge device in the Azure portal, but its easier from Visual Studio Code 
   with the Azure IoT Edge extension installed.

   There are 2 ways to create an Azure IoT Edge device from Visual Studio Code:

   Open the command palette and search for *Azure IoT Edge: Create IoT Edge Device*. 

   ​	**or**

   With the Explorer icon in the Visual Studio Code Activity Bar selected, go to the *AZURE IOT HUB* section in the 
   Explorer pane of Visual Studio Code. Select the "..." to open the Azure IoT Hub context menu. From the Context 
   Menu, choose *Create IoT Edge Device*. 

   Both options will open a prompt for you to enter the name of the device.

   > **Note:** There is also a *Azure IoT Hub: Create Device* command. This creates a basic IoT device definition, 
   which does not support the Azure IoT Edge Runtime and does not work with the Azure IoT Edge Simulator.

4. Configure Azure IoT Edge Simulator to use your Edge Device identity

   Again, there are 2 ways to create setup the Azure IoT Edge Simulator from within Visual Studio Code 

   Open the palette and search for *Azure IoT Edge: Setup IoT Edge Simulator*. After selecting the command, a list of 
   devices is displayed. Select the device you created in the previous step.

    **or**

   With the Explorer icon in the Visual Studio Code Activity Bar selected, go to the *AZURE IOT HUB* section in the 
   Explorer pane of Visual Studio Code. Expand the Devices list, and right click on the device you created in the 
   previous step to open the Context Menu. Select *Setup IoT Edge Simulator* from the Context Menu.

   This command will pass your Edge device credentials to the Azure IoT Edge Simulator via a command in the Terminal 
   Window.

   > **Note:** If you try to use the *Setup IoT Edge Simulator* command without first connecting to your Azure IoT Hub, 
   you will instead be prompted to enter the connection string for an Azure IoT Hub device.

5. Set environment variables

   The Azure IoT Edge solution deployment manifests (*deployment.template.json* and *deployment.debug.template.json*) 
   and module metadata files (*module.json*) support environment variable substitution. There are 3 environment 
   variable placeholders used in this sample - *$CONTAINER_REGISTRY_USERNAME*, *$CONTAINER_REGISTRY_PASSWORD* and 
   *$CONTAINER_REGISTRY_ADDRESS*. These are used to specify your container registry address and login credentials. 
   To run the code in the Azure IoT Edge Simulator, the *$CONTAINER_REGISTRY_ADDRESS* can be set to the Docker local 
   registry container value of *localhost:5000*. When using the local registry container value, the 
   $CONTAINER_REGISTRY_USERNAME and $CONTAINER_REGISTRY_PASSWORD are not used. However, since they are defined in the 
   deployment manifests, they must be defined in order to avoid the "Please set registry credential to .env file." 
   warning message on initial load.  
   To protect secrets, *.env* files should not be included in source control. Therefore, this sample includes a 
   *.env.temp* template file that can be renamed to *.env* or the values can be copied to your .env file. To build 
   and run the sample in the Azure IoT Edge Simulator, the following values can be used:

   ```text
   CONTAINER_REGISTRY_ADDRESS=localhost:5000
   CONTAINER_REGISTRY_USERNAME=<registry username>
   CONTAINER_REGISTRY_PASSWORD=<registry password>
   ```

   > **Note:** *CONTAINER_REGISTRY_USERNAME* and *CONTAINER_REGISTRY_PASSWORD* are not used with the local registry 
   container (*localhost:5000*), but these variables must be defined with any non-empty value.

   If you wish to deploy the solution to a real Edge device, make sure to set the values to your container registry. 

6. Verify Docker runtime mode (**Windows only**)

   This sample is built to run in an Ubuntu container and requires a Linux Container runtime. If running on Windows, 
   make sure that that Docker CE is running in the default Linux container mode, not Windows container mode.

   You can do this by right clicking the Docker icon in the system tray. If the context menu shows "Switch to Windows 
   containers...", Docker is running in Linux container mode.

7. Select your target architecture

   Currently, the Azure IoT Edge Visual Studio Code can build Azure IoT modules targeting *amd64* (Linux), *arm32* 
   (Linux) and *windows-amd64*. The target architecture setting tells the Azure IoT Edge extension which Dockerfile to 
   use in each module directory. This tutorial only included Dockerfiles that target Linux, so make sure the default 
   *amd64* is selected.

## Running the sample

1. Build and Run the solution

   Select the *deployment.template.json* file in the VS Code Explorer pane (file explorer) and right click to display 
   the context menu. Select *Build and Run IoT Edge Solution in Simulator*.

   This command issues *docker build* command for each module in the selected deployment manifest.

   The Azure IoT Edge Simulator status messages and any console messages from individual messages are also shown in the 
   Visual Studio integrated terminal. Once the *WebAppModule*has been initialized and started, IoT EdgeHub logging 
   messages will show in the VS Code Integrated terminal to indicate that data is being generated and sent to the 
   website.  Go to http://localhost:8080 and you will see the data displaying on the webpage.

   To stop the Azure IoT Edge Simulator after debugging, search for *Azure IoT Edge: Stop IoT Simulator* in the Visual 
   Studio command palette, or simple press `Ctrl+C` in the Visual Studio Code integrated terminal.

   Note: There is a 10 second delay for the site to get updated this way. This is by design to allow a human to have 
   time to register the changes. It is set in _ClientNotifier.cs_

2. Run the module in VS Code debug mode

   Included in _.vscode/launch.json_ are the VS Code settings to debug the WebApp Module locally. This is the first 
   option in the VS Code debug menu (or you can hit F5). Running in debug mode will have the same effect, for the 
   purpose of this demo, as running in the simulator.  As in the other methods to run the sample, the website to 
   see the results is http://localhost:8080.

### UI Data Generator

The data generator allows the WebApp Module UI to be tested without needing any other module to run.

There are 2 primary ways to use the data generator for UI work.

1) **Debugging in VSCode:** In launch.json, set the environment variable `UseInProcDataGenerator` to `true`.
2) **General Usage:** In deployment.template.json for the WebAppModule `createOptions`, change 
`UseInProcDataGenerator=FALSE` to `UseInProcDataGenerator=TRUE`.

By default for the sample, _UseInProcDataGenerator_ is set to TRUE in both locations. This way it is easy to see how 
the module works without modification.

## Connecting to Another Module

The route for another module to send data to the WebApp Module is: _webAppInput_

## Key concepts

This module is designed to take advantage of the features (security, queueing) of Azure IoT EdgeHub and display data 
received through that on the device. By locally displaying the information, there is no need to round-trip the data to 
the cloud and back to a web interface thus allowing data to be viewed offline and with self-contained code. 

When the WebApp receives data, it uses SignalR on the server and client side to update the data on the webpage in 
real-time. A version of SignalR is cached in the _modules/WebApp/wwwroot/js_ folder along with other Node.js packages 
used on the device. These cached versions of the packages are what need to be updated before deploying to the device 
should any changes be required.

HTTPS is not used by default on the module by default in _modules/WebApp/Program.cs_ although it is the default option 
with the ASP.NET Core template. To enable HTTPS requires additional configuration beyond the scope of this sample.

## Web Page Functionality

The HTML file provides the skeleton and CSS settings for the webpage. It also places the image on the website as an SVG 
background image. In _site.js_, bounding boxes and the text are drawn onto the background image. The dynamic creation 
of the bounding boxes and text allows them to be tailored to the specific image by updating the array of dictionaries 
with the required location and text information. As the program runs, jQuery is used to change the color of the 
bounding boxes and modify the text elements.

When changing the data array, the image, or changing to a different scenario, almost every file will need to change in 
some way. It is particularly easy to overlook a single change and cause the data to not display correctly or at all. 
Small changes that can be verified are recommended.