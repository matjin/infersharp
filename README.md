# InferSharp

**InferSharp** (also referred to as Infer#) is an interprocedural and scalable static code analyzer for C#. Via the capabilities of Facebook's [Infer](https://fbinfer.com/), this tool detects null pointer dereferences and resource leaks. Read more about our approach in the [Wiki page](https://github.com/microsoft/infersharp/wiki/InferSharp:-A-Scalable-Code-Analytics-Tool-for-.NET).

In addition to implementing the C# frontend, we contributed our [language-agnostic serialization layer](https://github.com/microsoft/infersharp/wiki/InferSharp:-A-Scalable-Code-Analytics-Tool-for-.NET#language-agnostic-representation-of-sil) ([Commit #1361](https://github.com/facebook/infer/commit/285ddb4a98f337a40d61e73b7a0867e44fa4f042)) to facebook/infer, which opens up opportunities for [additional language support](https://github.com/microsoft/infersharp/wiki/InferSharp:-A-Scalable-Code-Analytics-Tool-for-.NET#overview) in the future.

## Public Announcements
- [.NET Blog](https://devblogs.microsoft.com/dotnet/infer-interprocedural-memory-safety-analysis-for-c/)
- [Facebook Engineering Blog](https://engineering.fb.com/2020/12/14/open-source/infer/)
- [.NET Community Standup](https://youtu.be/cIB4gxqm6EY?list=PLdo4fOcmZ0oX-DBuRG4u58ZTAJgBAeQ-t&t=147)
- Visual Studio Toolbox - [YouTube](https://www.youtube.com/watch?v=yNSJv5wN4OA&feature=youtu.be), [Channel9](https://channel9.msdn.com/Shows/Visual-Studio-Toolbox/Analyzing-Code-with-Infer)

## Get Started

The latest version is ![GitHub release (latest by date including pre-releases)](https://img.shields.io/github/v/release/microsoft/infersharp?include_prereleases). Please refer to the [release page](https://github.com/microsoft/infersharp/releases) for more information on the changes.

### Windows Subsystem for Linux
The instructions on how to run Infer# on Windows via WSL2 can be found [here](/RUNNING_INFERSHARP_ON_WINDOWS.md).

### GitHub Action
The instructions on how to run Infer# as a GitHub Action can be found here: [Infer# Action](https://github.com/marketplace/actions/infersharp).

### Azure Pipelines
Infer# can be run as an Azure Pipelines [container job](https://docs.microsoft.com/en-us/azure/devops/pipelines/process/container-phases?view=azure-devops). An example can be found [here](/.build/azure-pipelines-example.yml).\
If the existing pipeline runs on Windows or running a multi-stage job is desired, refer to the example [here](/.build/azure-pipelines-example-multistage.yml).

### Docker Image
Alternatively, use our Docker image:
```shell
docker pull mcr.microsoft.com/infersharp:v1.0
```
Start a container in interactive mode, then run the following command in the container:
```shell
./run_infersharp.sh Examples
```

## Troubleshooting
Please refer to the [troubleshooting guide](TROUBLESHOOTING.md).

## Contributing

We welcome contributions. Please follow [this guideline](CONTRIBUTING.md).

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft's Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.

## Security Reporting Instructions

**Please do not report security vulnerabilities through public GitHub issues.** Instead, please follow [this guideline](SECURITY.md).
