# Toss Payments Integration Guide

> 이 문서는 Toss 결제 연동의 상세 흐름을 담는다.
> 솔루션 전체 요약은 루트 `README.md`를 참고한다.

## Overview

This app integrates Toss Payments for shipper-side payment confirmation.
The flow is:

1. Create a payment draft with `결제대기`
2. Send the client key to the frontend
3. Confirm the payment with Toss
4. Mark the request as paid
5. Create a `배차대기` record only after confirm succeeds

This follows the app policy that dispatch waiting records are not created before payment approval.

## Configuration

Required config section:

```json
"TossPayments": {
  "ClientKey": "test_ck_xxx",
  "SecretKey": "test_sk_xxx",
  "BaseUrl": "https://api.tosspayments.com"
}
```

`Program.cs` requires `TossPayments:SecretKey` at startup.

## Backend wiring

- `Services/TossPaymentsOptions.cs`
- `Services/TossPaymentsService.cs`
- `Controllers/Shipper/화주결제Controller.cs`

`TossPaymentsService` calls Toss `POST /v1/payments/confirm` with Basic auth using the secret key.

## API flow

### 1. Get Toss config

`GET /api/v1/payments/toss/config`

Response:

```json
{
  "clientKey": "test_ck_xxx",
  "baseUrl": "https://api.tosspayments.com",
  "isConfigured": true
}
```

### 2. Prepare payment

`POST /api/v1/payments/toss/prepare`

Request:

```json
{
  "의뢰Id": "REQ-001",
  "Amount": 50000
}
```

Behavior:

- validates the request exists
- rejects already completed requests
- reuses an existing pending payment when possible
- creates a new `결제` row with:
  - `결제상태 = 결제대기`
  - `OrderId = hongdal_{guid}`

Response includes:

- `결제Id`
- `의뢰Id`
- `OrderId`
- `Amount`
- `ClientKey`

### 3. Confirm payment

`POST /api/v1/payments/toss/confirm`

Request:

```json
{
  "PaymentKey": "payment_key_from_toss",
  "OrderId": "hongdal_xxx",
  "Amount": 50000
}
```

Behavior:

- validates the stored payment record
- checks amount match
- calls Toss confirm API
- on success:
  - stores `PaymentKey`
  - stores Toss response JSON
  - sets `결제상태 = 결제완료`
  - sets the shipper request to `결제완료`
  - sets `배차상태 = 매칭중`
  - creates `배차대기` if it does not already exist

## Data impact

### `결제`

Important fields:

- `결제Id`
- `의뢰Id`
- `화주Id`
- `PG사 = TossPayments`
- `결제수단`
- `결제상태`
- `결제금액`
- `OrderId`
- `PaymentKey`
- `Toss응답Json`
- `승인일시`

### `화주운송의뢰`

After confirm succeeds:

- `결제상태 = 결제완료`
- `배차상태 = 매칭중`

### `배차대기`

Created only after payment confirmation succeeds.

## Notes

- Existing completed payments are returned without creating duplicates.
- Existing `배차대기` rows are not recreated.
- The guide matches the current backend flow, so any future change to payment sequencing should update this document too.
