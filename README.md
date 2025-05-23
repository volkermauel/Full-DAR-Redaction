# Full-DAR-Redaction

This is the first full flavoured redaction tool with GUI, based on the command line version of the redaction tool.

The application now targets **.NET 8.0**.

## Environment setup

1. **Install the .NET 8 SDK**. On Ubuntu you can run:

   ```bash
   sudo apt-get update
   sudo apt-get install -y dotnet-sdk-8.0
   ```

   For other platforms see the [official .NET installation guides](https://learn.microsoft.com/dotnet/core/install).

2. **Build the solution** after cloning the repository:

   ```bash
   git clone <repository-url>
   cd Full-DAR-Redaction
   dotnet build Full-DAR-Redaction.sln
   ```

   Building the solution restores NuGet dependencies such as `DocumentFormat.OpenXml`.
