## 2.24.1 - Nova 2. Delivery 49. Hotfix 1 (February 17, 2025)
### What's changed
* LT-6053: Don't use settingsService expliciltly in brokers.


## 2.24.0 - Nova 2. Delivery 49 (February 07, 2025)
### What's changed
* LT-5992: Update rabbitmqbroker in margintrading.tradinghistory.


## 2.22.2 - Nova 2. Delivery 47. Hotfix 4 (February 5, 2025)
### What's changed
* LT-6033: Fix issue with directions (mobile/web request)


## 2.22.1 - Nova 2. Delivery 47. Hotfix 2 (January 15, 2025)
### What's changed
* LT-5991: Bump LykkeBiz.RabbitMqBroker to 8.11.1


## 2.23.0 - Nova 2. Delivery 48 (December 20, 2024)
### What's changed
* LT-5947: Update refit to 8.x version.
* LT-5887: Keep schema for appsettings.json up to date.
* LT-5876: Keep schema for appsettings.json up to date.
* LT-5875: Keep schema for appsettings.json up to date.
* LT-5730: Fix an issue with directions.


## 2.22.0 - Nova 2. Delivery 47 (November 18, 2024)
### What's changed
* LT-5852: Update messagepack to 2.x version.
* LT-5781: Add assembly load logger.
* LT-5765: Migrate to quorum queues.

### Deployment
In this release, all previously specified queues have been converted to quorum queues to enhance system reliability. The affected queues are:
- `lykke.mt.orderhistory.MarginTrading.TradingHistory.OrderHistoryBroker.DefaultEnv`
- `lykke.mt.position.history.MarginTrading.TradingHistory.PositionHistoryBroker.DefaultEnv.PositionsHistory`

#### Automatic Conversion to Quorum Queues
The conversion to quorum queues will occur automatically upon service startup **if**:
* There are **no messages** in the existing queues.
* There are **no active** subscribers to the queues.

**Warning**: If messages or subscribers are present, the automatic conversion will fail. In such cases, please perform the following steps:
1. Run the previous version of the component associated with the queue.
1. Make sure all the messages are processed and the queue is empty.
1. Shut down the component associated with the queue.
1. Manually delete the existing classic queue from the RabbitMQ server.
1. Restart the component to allow it to create the quorum queue automatically.

#### Poison Queues
All the above is also applicable to the poison queues associated with the affected queues. Please ensure that the poison queues are also converted to quorum queues.

#### Disabling Mirroring Policies
Since quorum queues inherently provide data replication and reliability, server-side mirroring policies are no longer necessary for these queues. Please disable any existing mirroring policies applied to them to prevent redundant configurations and potential conflicts.

#### Environment and Instance Identifiers
Please note that the queue names may include environment-specific identifiers (e.g., dev, test, prod). Ensure you replace these placeholders with the actual environment names relevant to your deployment. The same applies to instance names embedded within the queue names (e.g., DefaultEnv, etc.).


## 2.21.0 - Nova 2. Delivery 46 (September 27, 2024)
### What's changed
* LT-5594: Migrate to net 8.
* LT-5549: Failed to handle the message.


## 2.20.0 - Nova 2. Delivery 44 (August 19, 2024)
### What's changed
* LT-5510: Update rabbitmq broker library with new rabbitmq.client and templates.

### Deployment
Please ensure that the mirroring policy is configured on the RabbitMQ server side for the following queues:
- `lykke.mt.orderhistory.MarginTrading.TradingHistory.OrderHistoryBroker.DefaultEnv`
- `lykke.mt.position.history.MarginTrading.TradingHistory.PositionHistoryBroker.DefaultEnv.PositionsHistory`

These queues require the mirroring policy to be enabled as part of our ongoing initiative to enhance system reliability. They are now classified as "no loss" queues, which necessitates proper configuration. The mirroring feature must be enabled on the RabbitMQ server side.

In some cases, you may encounter an error indicating that the server-side configuration of a queue differs from the client’s expected configuration. If this occurs, please delete the queue, allowing it to be automatically recreated by the client.

**Warning**: The "no loss" configuration is only valid if the mirroring policy is enabled on the server side.

Please be aware that the provided queue names may include environment-specific identifiers (e.g., dev, test, prod). Be sure to replace these with the actual environment name in use. The same applies to instance names embedded within the queue names (e.g., DefaultEnv, etc.).


## 2.19.0 - Nova 2. Delivery 43 (June 03, 2024)
### What's changed
* LT-5505: - emir reporting wrong timestamp again.
* LT-5479: An error in the log "string or binary data would be truncated.".

### Deployment
* Since the changes affect all the components within git repository you'll have to redeploy all of them: TradingHistory, OrderHistoryBroker, PositionHistoryBroker.


## 2.18.4 - Nova 2. Delivery 42. Hotfix 4 (May 28, 2024)
### What's changed
* LT-5505: EMIR reporting wrong timestamp again

## 2.18.3 - Nova 2. Delivery 40. Hotfix 6 (May 28, 2024)
### What's changed
* LT-5505: EMIR reporting wrong timestamp again

## 2.18.2 - Nova 2. Delivery 42. Hotfix 2 (May 14, 2024)
### What's changed
* LT-5486: EMIR reporting - wrong EXECUTION-TIMESTAMP in Position file

### Deployment
* Stop the component
* Run the SQL script to migrate to datetime2 column type. Script file: ./scripts/20240507-datetime2-migration.sql.
* Run the component
* Ensure there are no errors in the log

## 2.18.1 - Nova 2. Delivery 40. Hotfix 4 (May 10, 2024)
### What's changed
* LT-5486: EMIR reporting - wrong EXECUTION-TIMESTAMP in Position file

## 2.18.0 - Nova 2. Delivery 40 (February 28, 2024)
### What's changed
* LT-5289:[PositionHistoryBroker] Step: deprecated packages validation is failed.
* LT-5288: [OrderHistoryBroker] Step: deprecated packages validation is failed.
* LT-5287: [TradingHistory] Step: deprecated packages validation is failed.
* LT-5255: Update lykke.httpclientgenerator to 5.6.2.

## 2.17.1 - Nova 2. Delivery 39. Hotfix 2 (February 7, 2024)
### What's changed
* LT-5246: Update vulnerable packages

## 2.17.0 - Nova 2. Delivery 39 (January 30, 2024)
### What's changed
* LT-5143: Ghangelog.md for trading history.
* LT-5142: Add most trading products api.

### Deployment
* Added a new endpoint: `/api/trades/most-traded-products`

## 2.16.0 - Nova 2. Delivery 38 (December 12, 2023)
### What's changed
* LT-5076: Sql timeout errors.
* LT-5047: Add directions filter to totalpnl api.


**Full change log**: https://github.com/lykkebusiness/margintrading.tradinghistory/compare/v2.15.0...v2.16.0

## 2.15.0 - Nova 2. Delivery 37 (October 18, 2023)
### What's changed
* LT-5018: Add new direction filter for closed positions.


**Full change log**: https://github.com/lykkebusiness/margintrading.tradinghistory/compare/v2.14.0...v2.15.0


## 2.14.0 - Nova 2. Delivery 36 (August 31, 2023)
### What's changed
* LT-4905: Update nugets.


**Full change log**: https://github.com/lykkebusiness/margintrading.tradinghistory/compare/v2.13.0...v2.14.0\

## 2.13.0 - Nova 2. Delivery 33 (April 10, 2023)

### What's changed
* LT-4548: Issue with displaying of swap for opening position today.


** Full change log: https://github.com/lykkebusiness/margintrading.tradinghistory/compare/v2.12.0...v2.13.0\


## 2.12.0 - Nova 2. Delivery 32 (March 1, 2023)

### What's changed
* LT-4519: Move getdealdetails from donut.
* LT-4377: Validateskipandtake implementation replace.


**Full change log**: https://github.com/lykkebusiness/margintrading.tradinghistory/compare/v2.10.1...v2.12.0
