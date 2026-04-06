# Complaint Ticket Policy and Implementation TODO

## 1) Muc tieu

Chuyen luong complaint tu free-form sang context-based:

- Moi ticket phai gan voi su kien nghiep vu cu the (payment, map access, gameplay result, reward, trial).
- Co cua so thoi gian mo ticket (SLA create window).
- Co chong spam va chong duplicate.

## 2) Pham vi ticket duoc phep tao

### Case A: PaymentIssue

- Mo ta: Da thanh toan (mua map/goi, nap coin) nhung quyen loi khong cap nhat hoac sai.
- Bat buoc context:
  - paymentRecordId (uu tien), hoac mapId/packageId neu khong co paymentRecordId.
- Dieu kien:
  - PaymentStatus = Completed.
  - Ticket duoc tao trong 7 ngay tu PaidAt.

### Case B: AccessIssue

- Mo ta: Da mua map/goi nhung khong truy cap duoc.
- Bat buoc context:
  - mapId hoac packageId.
- Dieu kien:
  - User co so huu hop le (PaymentRecord Completed hoac MyMap/UserPackage).
  - Ticket tao trong 7 ngay tu giao dich lien quan gan nhat.

### Case C: GameplayScoringIssue

- Mo ta: Choi xong thay status/score/stars sai.
- Bat buoc context:
  - submissionId hoac playHistoryId.
- Dieu kien:
  - Ban ghi thuoc dung user.
  - Ticket tao trong 72 gio tu EndTime.

### Case D: RewardBalanceIssue

- Mo ta: Cong/tru XP hoac OrbitCoin sai sau gameplay/giao dich.
- Bat buoc context:
  - xpTransactionId hoac orbitCoinTransactionId.
  - Fallback: submissionId + mapId neu transaction chua sinh.
- Dieu kien:
  - Co nguon phat sinh hop le (submission/payment lien quan).
  - Ticket tao trong 72 gio tu su kien.

### Case E: TrialIssue

- Mo ta: Free trial bi tru sai hoac bi chan sai.
- Bat buoc context:
  - mapId + playHistoryId.
- Dieu kien:
  - Map co FreeTrialAttemptLimit > 0.
  - Ticket tao trong 24 gio tu lan bi loi.

## 3) Cac case KHONG di vao Complaint

- Report noi dung map vi pham: su dung luong Community ReportMap.
- Gop y tinh nang chung: tao kenh Feedback rieng.

## 4) Rule chung bat buoc

- Authentication:
  - Chi Learner duoc tao complaint tu learner endpoint.
- Required payload chung:
  - category, subject, description.
  - context object theo category.
- Duplicate control:
  - Khong cho tao ticket moi neu da co Open/InProgress cung category + contextKey trong 72 gio.
- Rate limit:
  - Toi da 3 ticket/ngay/user (co the config).
- Workflow status:
  - Open -> InProgress -> Resolved.

## 5) De xuat model context

Them vao Complaint:

- ContextType (enum): Payment, Access, Gameplay, Reward, Trial
- ContextId (Guid?): id chinh theo case (paymentRecordId/submissionId/playHistoryId/...)
- MapId (Guid?)
- PackageId (Guid?)
- PaymentRecordId (Guid?)
- SubmissionId (Guid?)
- PlayHistoryId (Guid?)
- XpTransactionId (Guid?)
- OrbitCoinTransactionId (Guid?)
- OccurredAt (DateTime?): thoi diem su kien
- ContextKey (string): key de dedupe, vd category:contextId:userId

## 6) API contract de xuat

CreateComplaintRequest moi:

- subject: string
- category: enum (PaymentIssue|AccessIssue|GameplayScoringIssue|RewardBalanceIssue|TrialIssue)
- description: string
- context:
  - paymentRecordId?: Guid
  - mapId?: Guid
  - packageId?: Guid
  - submissionId?: Guid
  - playHistoryId?: Guid
  - xpTransactionId?: Guid
  - orbitCoinTransactionId?: Guid

Response tao ticket:

- complaintId
- acceptedCategory
- contextSummary
- createdAt

## 7) TODO List implement

### 7.1 Domain and DB

- [ ] Tao enum ComplaintCategoryEnum thay cho free string category.
- [ ] Bo sung cac cot context vao entity Complaint.
- [ ] Tao migration cho schema moi va index:
  - [ ] (UserId, ComplaintStatus)
  - [ ] (Category, ContextKey, ComplaintStatus)
  - [ ] (CreatedAt)
- [ ] Backfill cho du lieu cu (category string -> enum mapping), default Unknown neu khong map duoc.

### 7.2 Application validation

- [ ] Tao ComplaintPolicyService de validate theo tung category.
- [ ] Validate ownership theo context:
  - [ ] PaymentIssue/AccessIssue check PaymentRecord, MyMap, UserPackage.
  - [ ] GameplayScoringIssue/TrialIssue check Submission/UserMapPlayHistory cua user.
  - [ ] RewardBalanceIssue check XpTransaction/OrbitCoinTransaction cua user.
- [ ] Validate time window:
  - [ ] Payment, Access <= 7 ngay.
  - [ ] Gameplay, Reward <= 72 gio.
  - [ ] Trial <= 24 gio.
- [ ] Validate duplicate (Open/InProgress cung context trong 72 gio).
- [ ] Validate rate limit 3 ticket/ngay/user.

### 7.3 API and handlers

- [ ] Cap nhat CreateComplaintRequest + CreateComplaintCommand.
- [ ] Refactor CreateComplaintCommandHandler:
  - [ ] Chuyen category string -> enum.
  - [ ] Goi ComplaintPolicyService.
  - [ ] Luu context fields vao Complaint.
- [ ] Cap nhat swagger docs cho learner complaint endpoint.

### 7.4 CMS operations

- [ ] Hien thi contextSummary trong danh sach va detail complaint.
- [ ] Bo sung filter theo category, mapId, paymentRecordId, submissionId.
- [ ] Template response cho staff (phan loai theo case de xu ly nhanh).

### 7.5 FE tasks

- [ ] Chuyen form tao complaint sang dynamic form theo category.
- [ ] Chi hien cac ticket category hop le tu man hinh ngu canh:
  - [ ] Tu payment history -> PaymentIssue.
  - [ ] Tu map detail/play history -> GameplayScoringIssue/TrialIssue.
  - [ ] Tu wallet/xp history -> RewardBalanceIssue.
- [ ] Show message ro rang khi khong du dieu kien mo ticket.

### 7.6 Monitoring and rollout

- [ ] Log structured cho complaint validation fail reason.
- [ ] Dashboard metric:
  - [ ] So ticket theo category/ngay.
  - [ ] Ty le reject do policy.
  - [ ] Thoi gian xu ly trung binh.
- [ ] Rollout theo phase:
  - [ ] Phase 1: cho phep song song category cu + moi.
  - [ ] Phase 2: bat buoc category moi.

## 8) Acceptance criteria

- [ ] User khong the tao ticket neu khong co context hop le.
- [ ] User khong the tao ticket ngoai time window theo policy.
- [ ] User khong the tao duplicate ticket trong 72 gio.
- [ ] User khong the vuot qua rate limit/ngay.
- [ ] Staff nhin thay context ro rang de xu ly ma khong can hoi lai thong tin co ban.

## 9) Ghi chu mapping nhanh theo ngu canh

- Sau mua map/goi va loi cap quyen -> AccessIssue.
- Sau payment thanh cong nhung sai so lieu -> PaymentIssue.
- Sau choi map thay score/stars/status sai -> GameplayScoringIssue.
- Sau choi map hoac giao dich thay XP/coin sai -> RewardBalanceIssue.
- Loi lien quan free trial -> TrialIssue.
