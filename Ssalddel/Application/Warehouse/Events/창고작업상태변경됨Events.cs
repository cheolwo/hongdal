namespace Ssalddel.Application.Warehouse.Events;

public sealed record 창고입고완료됨Event(
    string 사용자Id,
    string 역할명,
    long 입고Id,
    int 생성상품수,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;

public sealed record 창고입고검수완료됨Event(
    string 사용자Id,
    string 역할명,
    long 입고상품Id,
    int 가용수량,
    int 불량수량,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;

public sealed record 창고적재위치배정됨Event(
    string 사용자Id,
    string 역할명,
    long 입고상품Id,
    string 보관위치,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;

public sealed record 창고포장완료됨Event(
    string 사용자Id,
    string 역할명,
    long 입고상품Id,
    int 포장수량,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;

public sealed record 창고피킹완료됨Event(
    string 사용자Id,
    string 역할명,
    string 피킹작업Key,
    long? 창고Id,
    int 피킹수량,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;

public sealed record 창고재위탁운송생성됨Event(
    string 사용자Id,
    string 역할명,
    long 입고상품Id,
    int 요청수량,
    string 의뢰Id,
    string Route,
    string TraceId,
    DateTime 발생시각Utc,
    string AppKey) : INotification;
