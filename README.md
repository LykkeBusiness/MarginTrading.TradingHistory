# MarginTrading.TradingHistory API, OrderHistoryBroker, PositionHistoryBroker #

API to get trading history. Brokers to pass historical data from message queue to storage.
Below is the API description.

## How to use in prod env? ##

1. Pull "mttradinghistoryapi" docker image with a corresponding tag.
2. Configure environment variables according to "Environment variables" section.
3. Put secrets.json with endpoint data including the certificate:
```json
{
  "Kestrel": {
    "EndPoints": {
      "HttpsInlineCertFile": {
        "Url": "https://*:5041",
        "Certificate": {
          "Path": "<path to .pfx file>",
          "Password": "<certificate password>"
        }
      }
    }
  }
}
```
4. Initialize all dependencies.
5. Run.

## How to run for debug? ##

1. Clone repo to some directory.
2. In MarginTrading.TradingHistory root create a appsettings.dev.json with settings.
3. Add environment variable "SettingsUrl": "appsettings.dev.json".
4. VPN to a corresponding env must be connected and all dependencies must be initialized.
5. Run.

### Dependencies ###

TBD

### Configuration ###

Kestrel configuration may be passed through appsettings.json, secrets or environment.
All variables and value constraints are default. For instance, to set host URL the following env variable may be set:
```json
{
    "Kestrel__EndPoints__Http__Url": "http://*:5040"
}
```

### Environment variables ###

* *RESTART_ATTEMPTS_NUMBER* - number of restart attempts. If not set int.MaxValue is used.
* *RESTART_ATTEMPTS_INTERVAL_MS* - interval between restarts in milliseconds. If not set 10000 is used.
* *SettingsUrl* - defines URL of remote settings or path for local settings.

### Settings ###

TradingHistoryService settings schema is:
<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./service.json) -->
<!-- The below code snippet is automatically added from ./service.json -->
```json
{
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "ENVIRONMENT": "String",
  "IsLive": "Boolean",
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "String"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Microsoft": "String"
    }
  },
  "serilog": {
    "minimumLevel": {
      "default": "String"
    }
  },
  "TradingHistoryClient": {
    "ApiKey": "String",
    "ServiceUrl": "String"
  },
  "TradingHistoryService": {
    "ApiKey": "String",
    "Db": {
      "HistoryConnString": "String",
      "LogsConnString": "String",
      "OrderBlotterExecutionTimeout": "DateTime",
      "StorageMode": "String"
    },
    "UseSerilog": "Boolean"
  },
  "TZ": "String"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->

OrderHistoryBroker settings schema is:
<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./orderHistoryBroker.json) -->
<!-- The below code snippet is automatically added from ./orderHistoryBroker.json -->
```json
{
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "ENVIRONMENT": "String",
  "IsLive": "Boolean",
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "String"
      }
    }
  },
  "serilog": {
    "Enrich": [
      "String"
    ],
    "minimumLevel": {
      "default": "String"
    },
    "Properties": {
      "Application": "String"
    },
    "Using": [
      "String"
    ],
    "writeTo": [
      {
        "Args": {
          "configure": [
            {
              "Args": {
                "outputTemplate": "String"
              },
              "Name": "String"
            }
          ]
        },
        "Name": "String"
      }
    ]
  },
  "TZ": "String"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->

PositionHistoryBroker settings schema is:
<!-- MARKDOWN-AUTO-DOCS:START (CODE:src=./positionHistoryBroker.json) -->
<!-- The below code snippet is automatically added from ./positionHistoryBroker.json -->
```json
{
  "APP_UID": "Integer",
  "ASPNETCORE_ENVIRONMENT": "String",
  "ENVIRONMENT": "String",
  "IsLive": "Boolean",
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "String"
      }
    }
  },
  "serilog": {
    "Enrich": [
      "String"
    ],
    "minimumLevel": {
      "default": "String"
    },
    "Properties": {
      "Application": "String"
    },
    "Using": [
      "String"
    ],
    "writeTo": [
      {
        "Args": {
          "configure": [
            {
              "Args": {
                "outputTemplate": "String"
              },
              "Name": "String"
            }
          ]
        },
        "Name": "String"
      }
    ]
  },
  "TZ": "String"
}
```
<!-- MARKDOWN-AUTO-DOCS:END -->
