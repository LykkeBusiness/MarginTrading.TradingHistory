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
