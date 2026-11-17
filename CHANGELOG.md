# Changelog

All notable changes to this project are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

### fretje.Aspire.Hosting.LocalEventing

#### Added

- `AddBlobStorageEventProxy`: adds a blob proxy in front of Azurite that forwards blob operations and sends Event Grid notifications.
- `AddEventGridEmulator`: adds a local Event Grid emulator container.
- `AddTopic`: declares a topic on the Event Grid emulator.
- `WithBlobTriggerSubscription`: subscribes an Azure Functions blob trigger (EventGrid source) to a topic.
- `WithEventGridTriggerSubscription`: subscribes an Azure Functions `[EventGridTrigger]` to a topic.
- `WithWebhookSubscription`: forwards a topic's events to an arbitrary webhook.

#### Changed

- Release tags now use the `localeventing-v` prefix (for example `localeventing-v1.0.0`) instead of `v`, so every package in the repository can be versioned and released independently.

### fretje.Aspire.Hosting.Azure.SignalR

#### Added

- `WithUpstream`: adds an upstream endpoint (any URL, or a path on another resource's endpoint) that the local Azure SignalR emulator forwards serverless events to. Can be called multiple times.
- `WithUpstreamFunctionsProject`: adds the Azure Functions SignalR trigger webhook of a Functions resource as an upstream endpoint.
