# Complaint CMS Flow v2 – FE Mapping & UI Validation

Tài liệu này dùng cho team FE/CMS và QA để bám đúng flow khiếu nại mới.

## 1) Trạng thái chính (v2)

- `Open`
- `SellerPending`
- `FixInProgress`
- `FixSubmitted`
- `Verified`
- `SellerRejected`
- `SellerNoResponse`
- `ResolvedRefund`
- `ResolvedReject`
- `Closed`

> Dữ liệu cũ có thể còn `InProgress` / `Resolved` (legacy compatibility).

## 2) Endpoint CMS cần dùng

- `GET /api/cms/complaints`
- `GET /api/cms/complaints/{complaintId}`
- `POST /api/cms/complaints/{complaintId}/messages`
- `POST /api/cms/complaints/{complaintId}/status`

## 3) FE action -> toStatus (CMS Moderator/Admin)

| UI action | Current status | toStatus | issueRefund |
|---|---|---|---|
| Yêu cầu seller phản hồi | Open | SellerPending | false |
| Đánh dấu seller không phản hồi | Open / SellerPending / FixInProgress | SellerNoResponse | false |
| Xác nhận seller đã nhận lỗi và sửa | SellerPending | FixInProgress | false |
| Xác nhận seller đã nộp fix | FixInProgress | FixSubmitted | false |
| Chuyển buyer verify | FixSubmitted | Verified | false |
| Kết luận hoàn tiền | Verified / SellerRejected / SellerNoResponse | ResolvedRefund | true |
| Kết luận không hoàn tiền | Verified / SellerRejected / SellerNoResponse | ResolvedReject | false |
| Đóng ticket | ResolvedRefund / ResolvedReject | Closed | false |

## 4) UI validation theo state machine

FE nên chỉ hiển thị những action hợp lệ theo `currentStatus`:

- `Open`: `SellerPending`, `SellerNoResponse`
- `SellerPending`: `FixInProgress`, `SellerRejected`, `SellerNoResponse`
- `FixInProgress`: `FixSubmitted`, `SellerNoResponse`
- `FixSubmitted`: `Verified`
- `Verified`: `ResolvedRefund`, `ResolvedReject`
- `SellerRejected`: `ResolvedRefund`, `ResolvedReject`
- `SellerNoResponse`: `ResolvedRefund`, `ResolvedReject`
- `ResolvedRefund`: `Closed`
- `ResolvedReject`: `Closed`
- `Closed`: không action

## 5) Payload mẫu cho endpoint đổi trạng thái

```json
{
  "toStatus": 9,
  "note": "Validated issue from evidence, full refund approved",
  "issueRefund": true
}
```

## 6) Quy ước FE để tránh lỗi

- Nếu chọn `ResolvedRefund`, đặt `issueRefund=true`.
- Nếu chọn `ResolvedReject`, đặt `issueRefund=false`.
- Không gửi `Open` như target status (trừ trường hợp đặc biệt do backend không cho quay ngược flow).
- Sau mỗi lần đổi status thành công, FE nên gọi lại detail để sync timeline/status history.

## 7) Xử lý lỗi thường gặp

- `400 ValidationFailed`: chuyển trạng thái không hợp lệ theo flow.
- `401 Unauthorized`: thiếu hoặc hết hạn token.
- `403 Forbidden`: không phải Admin/Moderator.
- `404 NotFound`: complaint không tồn tại.
