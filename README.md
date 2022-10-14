# NiCE.Community.TeamsCallRecordWebhook
This C# Project can be used to test the functionality of the MSGraph Api change notification system in connection with ``CallRecord`` from Teams.

## Prerequisites
- Visual Studio 2022 with ``ASP.NET`` development toolchain installed
- A Microsoft Tenant with permissions to register an application

## Setup
- Register an application in the AAD of a Tenant with the ``CallRecords.Read.All`` Application Permission
- In ``appsettings.json`` ``WebHook:PublicEndpoint`` need to be set to an endpoint that is public reachable to receive the change notification by using a webhook
- Build and start ``NiCE.Community.TeamsCallRecordWebhook``
- Call the endpoint ``GET api/subscription/register`` to set up the subscription for the change notifications:
  - tenantId: guid (the directory/tenant id)
  - resource: "CallRecord"
  - resourceUrl: "/communications/callRecords"
  - applicationId: guid (application/client id)
  - applicationSecret: string (secret of the application)