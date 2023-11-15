# Changelog

## 2.1.0
- Improve streaming performance
- Improve connection reliability in some cases
- Implement ServerCallContext.Peer ([#37](https://github.com/cyanfish/grpc-dotnet-namedpipes/issues/37))
- Set cancellation token on client disconnect ([#30](https://github.com/cyanfish/grpc-dotnet-namedpipes/issues/30))
- The [readme](https://github.com/cyanfish/grpc-dotnet-namedpipes) now has a comparison matrix for ASP.NET gRPC

## 2.0.0
- Add macOS and Linux support
- Change build targets to: net462, net6, netstandard2.0
- Bump assembly version
- Set a default connection timeout of 30s (instead of unlimited)
- Add NamedPipeServer.Error event for previously unlogged errors

## 1.4.4
- Add strong naming to the assembly

## 1.4.2
- Throw an exception when starting an already-killed server

## 1.4.1
- Fix server cleanup issues

## 1.4.0
- Fix cancellation issues

## 1.3.0
- Add a .NET 5 build with support for pipe security options

## 1.2.0
- Update to newer gRPC API (2.32)

## 1.1.2
- Fix gRPC dependency version range to &lt;2.32

## 1.1.1
- Update project metadata

## 1.1.0
- Add a ConnectionTimeout client option (defaults to infinite)

## 1.0.2
- Improve server connection error handling

## 1.0.1
- Initial public release with core functionality