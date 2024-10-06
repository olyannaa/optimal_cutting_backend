# optimal_cutting_backend
## Deploying backend in development via visual studio
### Getting started
To start with, make sure that virualization is abled on your pc and you have docker installed on it.

**Useful links:**

[Microsoft's Guide to Enabling Virtualization](https://support.microsoft.com/ru-ru/windows/включение-виртуализации-в-windows-c5578302-6e43-4b4b-a449-8ced115f58e1)

[Docker's Main Page](https://www.docker.com)

### Preparations

1) Install neccessary extensions for visual studio:
    + C# [extension ID: ms-dotnettools.csharp]
    + Docker [extension ID: ms-azuretools.vscode-docker].
2) Clone repository to your local machine

### Project Setup

3) Copy and replace *appsettings.json* in a project's directory
4) Navigate to *vega.csproj* folder via visual studio terminal
```
cd *your local path to folder*
```
or
```
cd optimal_cutting_backend
cd optimal_cutting_backend
```
5) Generate SSL certificates for development
```
dotnet dev-certs https --clear
dotnet dev-certs https -ep $env:USERPROFILE\.aspnet\https\aspnetapp.pfx -p *password*
dotnet dev-certs https --trust
```
if dev-certs utility is not installed use command:
```
dotnet tool install --global dotnet-dev-certs
```
6) Run docker on your pc and navigate to *docker-compose.yml* folder via visual studio terminal
```
cd *your local path to folder*
```
or
```
cd ..
cd ..
```
