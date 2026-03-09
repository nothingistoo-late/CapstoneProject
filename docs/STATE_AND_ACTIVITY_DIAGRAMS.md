# State Machine & Activity Diagrams (Draw.io / PlantUML)

The diagrams below use **PlantUML**. In draw.io: **Arrange → Insert → Advanced → PlantUML** (or **+ → Advanced → PlantUML**), paste each code block and insert.

---

## 1. State Machine Diagrams (Main Entities)

### 1.1 Map (MapStatusEnum)

Map UGC state flow: Draft → Submit for review → Approve/Reject → Publish.

```plantuml
@startuml
title State Machine: Map (MapStatus)
state "Draft" as Draft
state "PendingReview" as PendingReview
state "Approved" as Approved
state "Rejected" as Rejected
state "Published" as Published

[*] --> Draft
Draft --> PendingReview : Submit for review
PendingReview --> Approved : Admin/Moderator approves
PendingReview --> Rejected : Admin/Moderator rejects
Approved --> Published : Publish to catalog
Rejected --> Draft : Author edits and resubmits

note right of Draft
  Visible to author only
end note
note right of Published
  Shown in catalog;
  users can buy/play
end note
@enduml
```

---

### 1.2 Payment Record (PaymentStatusEnum)

Payment transaction status (PayOS, OrbitCoin, map/package purchase).

```plantuml
@startuml
title State Machine: PaymentRecord (PaymentStatus)
state "Pending" as Pending
state "Completed" as Completed
state "Failed" as Failed
state "Cancelled" as Cancelled
state "Refunded" as Refunded

[*] --> Pending
Pending --> Completed : Payment success
Pending --> Failed : Payment failed
Pending --> Cancelled : User/system cancels
Completed --> Refunded : Refund processed

@enduml
```

---

### 1.3 Map Report (ReportStatusEnum)

Map content report status, handled by Admin/Moderator.

```plantuml
@startuml
title State Machine: MapReport (ReportStatus)
state "Pending" as Pending
state "Reviewed" as Reviewed
state "Resolved" as Resolved
state "Dismissed" as Dismissed

[*] --> Pending
Pending --> Reviewed : Admin/Moderator reviews
Reviewed --> Resolved : Action taken (e.g. unpublish map)
Reviewed --> Dismissed : Report dismissed

@enduml
```

---

### 1.4 Room (RoomStatusEnum)

Competitive match room status.

```plantuml
@startuml
title State Machine: Room (RoomStatus)
state "Waiting" as Waiting
state "Playing" as Playing
state "Finished" as Finished
state "Cancelled" as Cancelled

[*] --> Waiting
Waiting --> Playing : Match starts
Playing --> Finished : All submissions / time up
Waiting --> Cancelled : Cancel room
Playing --> Cancelled : Cancel (edge case)

@enduml
```

---

### 1.5 Submission (SubmissionStatusEnum)

Submission (solution) status when the user runs a map.

```plantuml
@startuml
title State Machine: Submission (ResultStatus)
state "Pending" as Pending
state "Running" as Running
state "Accepted" as Accepted
state "WrongAnswer" as WrongAnswer
state "TimeLimitExceeded" as TimeLimitExceeded
state "ConstraintViolation" as ConstraintViolation
state "InternalError" as InternalError

[*] --> Pending
Pending --> Running : Execution starts
Running --> Accepted : Pass all tests
Running --> WrongAnswer : Wrong result
Running --> TimeLimitExceeded : Timeout
Running --> ConstraintViolation : Constraint violated
Running --> InternalError : System error

@enduml
```

---

## 2. Activity Diagrams / Flowcharts (Main Business Flows)

Main business flows with Actors; internal application processing is not detailed.

---

### 2.1 Deposit → OrbitCoin via PayOS

User deposits real money; system (PayOS webhook or confirm) credits OrbitCoin to wallet.

```plantuml
@startuml
title Activity: User Deposit → OrbitCoin (PayOS)
|User|
start
:Request deposit amount;
|System|
:Create deposit order (PayOS);
:Return payment link;
|User|
:Redirect to PayOS;
:Pay on PayOS;
|#LightBlue|PayOS|
:Process payment;
:Notify webhook / redirect;
|System|
:Verify webhook or confirm;
:Credit OrbitCoin to User Wallet;
|User|
:Receive OrbitCoin;
stop
@enduml
```

---

### 2.2 Purchase Map with OrbitCoin

User (Buyer) purchases map; Seller receives amount minus platform fee.

```plantuml
@startuml
title Activity: Purchase Map (OrbitCoin)
|Buyer|
start
:Choose map to buy;
:Request purchase (OrbitCoin);
|System|
:Check balance & ownership;
:Deduct OrbitCoin from Buyer;
:Transfer (price - fee) to Seller;
:Record payment & ownership;
|Seller|
:Receive OrbitCoin (after fee);
|Buyer|
:Map unlocked for play;
stop
@enduml
```

---

### 2.3 Purchase Package (membership)

User purchases Free/Pro/Creator package with OrbitCoin.

```plantuml
@startuml
title Activity: Purchase Package
|User|
start
:Choose package (Free/Pro/Creator);
:Request purchase (OrbitCoin);
|System|
:Check OrbitCoin balance;
:Deduct OrbitCoin;
:Activate UserPackage (duration, limits);
:Record PaymentRecord;
|User|
:Package benefits active;
stop
@enduml
```

---

### 2.4 Create and Publish Map (UGC) – with approval

Creator creates map and submits for review; Admin/Moderator approves or rejects; only Admin/Moderator can publish an Approved map to the catalog. If rejected, this flow ends; the creator may start a new submission (edit and resubmit) from the application.

```plantuml
@startuml
title Activity: Create & Publish Map (UGC with approval)
|Creator|
start
:Create map (Draft);
:Upload map detail JSON, hints, tags;
:Submit for review;
|System|
:Set map status to PendingReview;
|Admin/Moderator|
:Review map;
if (Approve?) then (yes)
  partition "Approval path" {
    |Admin/Moderator|
    :Approve and publish map to catalog;
    |System|
    :Set map status to Published;
    stop
  }
else (no)
  partition "Rejection path" {
    |System|
    :Set map status to Rejected;
    |Creator|
    :End of this submission;
    stop
  }
endif
@enduml
```

---

### 2.5 Play Map and Submit Solution (Play & Submit)

User plays map (owned or free), submits solution; system evaluates and updates XP/Stars.

```plantuml
@startuml
title Activity: Play Map & Submit Solution
|User|
start
:Select map (owned or free);
|System|
:Check access (package / ownership);
|User|
:Play map;
:Submit solution;
|System|
:Run submission (engine);
:Evaluate (Accepted / WrongAnswer / Timeout / ...);
:Update UserMapResult (stars, best score);
:Grant XP (XpTransaction);
|User|
:View result & progress;
stop
@enduml
```

---

### 2.6 Report Map → Admin handling

User reports map violation; Admin/Moderator reviews and resolves (Resolved / Dismissed). If dismissed, this flow ends; the user may submit a new report from the application.

```plantuml
@startuml
title Activity: Report Map → Admin Review
|User|
start
:Report map (reason, content);
|System|
:Create MapReport and set status to Pending;
|Admin/Moderator|
:View report list;
:Review report and map content;
if (Valid?) then (yes)
  partition "Valid report path" {
    :Unpublish map, warn creator,\nor apply other moderation action;
    |System|
    :Mark report as Resolved;
    stop
  }
else (no)
  partition "Dismissed path" {
    |System|
    :Mark report as Dismissed;
    |User|
    :End of this report;
    stop
  }
endif
@enduml
```

---

### 2.7 Join Competitive Room / Match

User creates or joins a room, waits for enough players, match starts, submit solution, finish and view results.

```plantuml
@startuml
title Activity: Join Competitive Room / Match
|User|
start
:Create room or join existing room;
|System|
:Room status = Waiting;
|User|
:Wait for enough participants;
|System|
:Start match (Room → Playing);
:Broadcast map / start;
|User|
:Play and submit solution;
|System|
:Collect submissions;
:Evaluate & rank (UserMatchResult);
:Room → Finished;
|User|
:View ranking and result;
stop
@enduml
```

---

## How to use in draw.io

1. Open draw.io.
2. **Insert → Advanced → PlantUML** (or **+ → Advanced → PlantUML** depending on version).
3. Copy the full content of one code block (from `@startuml` to `@enduml`), paste into the PlantUML box.
4. Click **Insert** to generate the diagram.
5. Repeat for each diagram you need.

If PlantUML is not available in draw.io, install the **PlantUML** add-on (Extensions / Integrations).
