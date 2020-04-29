## 1.13.15 (TBD)

* BUGS-1635 (TBD): Trades duplication

The following sql script has to be executed against database:

```sql
-- Update Trades table
with cte as (
    select OID,
           Id,
           AccountId,
           AssetPairId,
           OrderCreatedDate,
           ROW_NUMBER()
                   over (partition by Id, AccountId, AssetPairId, OrderCreatedDate order by Id, AccountId, AssetPairId, OrderCreatedDate) row_num
    from Trades)
delete
from cte
where row_num > 1
go


create unique index IX_Trades_Id_AccountId_AssetPairId_OrderCreatedDate
	on Trades (Id, AccountId, AssetPairId, OrderCreatedDate)
go

-- Update Deals table
with cte as (
    select OID,
           DealId,
           AccountId,
           AssetPairId,
           Direction,
           Volume,
           Created,
           ROW_NUMBER()
                   over (partition by DealId, AccountId, AssetPairId, Direction, Volume, Created order by DealId, AccountId, AssetPairId, Direction, Volume, Created) row_num
    from Deals)
delete
from cte
where row_num > 1
go

create unique index IX_Deals_DealId_AccountId_AssetPairId_Direction_Volume_Created
	on Deals (DealId, AccountId, AssetPairId, Direction, Volume, Created)
go

-- Update PositionHistory
with cte as (
    select OID,
           DealId,
           AccountId,
           AssetPairId,
           Direction,
           Volume,
           HistoryTimestamp,
           row_number()
                   over (partition by DealId, AccountId, AssetPairId, Direction, Volume, HistoryTimestamp order by DealId, AccountId, AssetPairId, Direction, Volume, HistoryTimestamp) row_num
    from PositionsHistory)
delete
from cte
where row_num > 1
go

create unique index IX_PositionHistory_DealId_AccountId_AssetPairId_Direction_Volume_HistoryTimestamp
	on PositionsHistory (DealId, AccountId, AssetPairId, Direction, Volume, HistoryTimestamp)
go

```