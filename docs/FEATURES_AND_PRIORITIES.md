# 🎯 HỆ THỐNG SOẠN GIÁO ÁN VÀ ÔN TẬP HÓA HỌC

## 🔥 PRIORITY HIGH (Core Features - Phải có)

### 1. **Quản lý Ngân hàng Câu hỏi Hóa học**
**Mục đích:** Xây dựng kho câu hỏi chất lượng cao, phân loại chi tiết

**Features:**
- ✅ CRUD câu hỏi trắc nghiệm (4 đáp án)
- ✅ Phân loại:
  - Theo cấu trúc: Chương/Bài/Bài tập cụ thể
  - Theo độ khó: Dễ/TB/Khó
  - Theo chủ đề: ChemistryTopic (Hóa vô cơ/Hữu cơ/Phân tích)
  - Theo năng lực: Nhận biết/Thông hiểu/Vận dụng/Vận dụng cao
- ✅ Upload hình ảnh (công thức, phương trình, sơ đồ)
- ✅ Giải thích chi tiết cho mỗi câu
- ✅ Tags và keywords cho tìm kiếm
- ✅ Import từ Excel template
- ✅ Question versioning (track changes)

**Entities:** `QuestionBank`, `Question`, `QuestionOption`, `ChemistryTopic`

**Tại sao quan trọng:** Backbone của cả hệ thống, ảnh hưởng đến exam và practice

---

### 2. **AI Tạo Câu hỏi Tự động**
**Mục đích:** Giảm workload cho giảng viên, tăng số lượng câu hỏi

**Features:**
- ✅ Tích hợp OpenAI GPT-4 cho lý thuyết
- ✅ Tích hợp Google Gemini cho tính toán
- ✅ Custom prompts cho Hóa học:
  - Nhập nội dung/công thức → AI tạo câu hỏi
  - Chọn độ khó, số lượng
  - AI tự động generate 4 đáp án (1 đúng, 3 nhiễu hợp lý)
- ✅ Teacher review & edit trước khi lưu
- ✅ Track token usage & cost
- ✅ Quality scoring (teacher feedback)

**Entities:** `AIQuestionGenerationRequest`, `AIExamGenerationRequest`

**Tech Stack:** OpenAI API, Google Gemini API, async processing

**Tại sao quan trọng:** USP của sản phẩm, tiết kiệm thời gian lớn

---

### 3. **Tạo Đề Thi & Thi thử**
**Mục đích:** Học sinh ôn tập, kiểm tra năng lực

**Features:**
- ✅ Tạo đề thi:
  - Chọn câu hỏi từ Question Bank
  - Hoặc AI auto-generate theo criteria
  - Set thời gian, tổng điểm
  - Phân bố độ khó (30% dễ, 50% TB, 20% khó)
  - Trộn thứ tự câu hỏi
- ✅ Làm bài thi:
  - **Offline support (PWA)** - mạng rớt vẫn làm được
  - Auto-save progress mỗi 30s
  - Timer countdown
  - Review trước khi nộp
- ✅ Chấm điểm tự động
- ✅ Xem kết quả chi tiết (đúng/sai từng câu)
- ✅ Analytics: điểm TB, phân bố, câu khó nhất
- ✅ Export kết quả (PDF)

**Entities:** `Exam`, `ExamQuestion`, `StudentAttempt`, `StudentAnswer`

**Tech Stack:** PWA, IndexedDB (offline), WebSocket (real-time)

**Tại sao quan trọng:** Core value cho học sinh

---

### 4. **Authen & Authorization**
**Mục đích:** Bảo mật, phân quyền

**Features:**
- ✅ Login/Register (Email, Google OAuth)
- ✅ Role-based: Admin, Teacher, Student
- ✅ Teacher:
  - CRUD giáo án, câu hỏi, đề thi
  - Xem analytics học sinh
- ✅ Student:
  - Xem bài học
  - Làm bài tập, thi thử
  - Xem kết quả
- ✅ JWT token authentication
- ✅ Password reset, email verification

**Entities:** `AppUser`, `AppRole`, `TeacherProfile`, `StudentProfile`

**Tech Stack:** ASP.NET Identity, JWT

**Tại sao quan trọng:** Bảo mật cơ bản, phải có từ đầu

---

## 🟡 PRIORITY MEDIUM (Important Features)

### 5. **Soạn Giáo án Điện tử**
**Mục đích:** Giảng viên soạn giáo án theo chuẩn Bộ GD

**Features:**
- ✅ Template giáo án (5 bước, 3 hoạt động)
- ✅ Link với Lesson entity
- ✅ Rich text editor (TinyMCE/CKEditor)
- ✅ **Export Word (.docx)** với format chuẩn VN
- ✅ AI suggest content (GPT-4)
- ✅ Version control (v1, v2, v3...)
- ✅ Chia sẻ giáo án với đồng nghiệp
- ✅ Clone & customize

**Entities:** `LessonPlan`, `LessonPlanTemplate`, `Lesson`

**Tech Stack:** Open XML SDK (Word export), TinyMCE

**Tại sao medium:** Quan trọng nhưng không phải core value cho học sinh

---

### 6. **Quản lý Nội dung Học liệu**
**Mục đích:** Tổ chức kiến thức theo sách giáo khoa

**Features:**
- ✅ Cấu trúc: Curriculum → Chapter → Lesson → Practice
- ✅ CRUD Chương trình (Hóa 10, 11, 12)
- ✅ CRUD Chương (Chương 1, 2, 3...)
- ✅ CRUD Bài học (Bài 1, 2, 3...)
- ✅ CRUD Bài tập (Exercise, Experiment, Quiz)
- ✅ **Upload tài liệu** (PDF, Word, PPT) - **ĐÁP ÁN: CÓ**
  - Tài liệu lý thuyết (PDF)
  - Slide bài giảng (PPT)
  - Tóm tắt kiến thức (Word)
- ✅ Upload video bài giảng (link YouTube/Vimeo)
- ✅ Upload hình ảnh minh họa
- ✅ Thứ tự hiển thị, metadata

**Entities:** `Curriculum`, `Chapter`, `Lesson`, `Practice`

**Storage:** Azure Blob Storage / AWS S3 cho files

**Tại sao medium:** Infrastructure quan trọng nhưng không phải feature nổi bật

---

### 7. **Hệ thống Bài tập & Luyện tập**
**Mục đích:** Học sinh làm bài tập theo từng bài học

**Features:**
- ✅ Practice types: Exercise, Experiment, Quiz, Review
- ✅ Link với Lesson
- ✅ Câu hỏi practice (từ Question Bank)
- ✅ Gợi ý, hướng dẫn từng bước
- ✅ Xem lời giải sau khi làm
- ✅ Track progress (đã làm chưa)
- ✅ Không giới hạn số lần làm
- ✅ Xem lịch sử làm bài

**Entities:** `Practice`, `Question`, `StudentAnswer`

**Tại sao medium:** Bổ trợ cho thi thử, không phải core

---

## 🟢 PRIORITY LOW (Nice to have)

### 8. **Analytics & Reports**
**Features:**
- Điểm trung bình của lớp
- Câu hỏi khó nhất (tỷ lệ sai cao)
- Progress tracking học sinh
- Export báo cáo (Excel, PDF)

**Entities:** `LearningAnalytics`

---

### 9. **Hệ thống Subscription & Payment**
**Features:**
- Free tier: 50 câu hỏi, 5 đề thi
- Premium: Unlimited, AI unlimited
- VNPay/Momo integration

**Entities:** `Subscription`, `SubscriptionPlan`, `Payment`

---

### 10. **Diễn đàn Hỏi đáp**
**Features:**
- Học sinh hỏi, giảng viên trả lời
- Upvote/downvote
- Tag theo chủ đề

**Entities:** `ForumPost`, `ForumComment` (chưa có)

---

### 11. **Mobile App**
**Features:**
- React Native / Flutter
- Làm bài thi trên mobile
- Push notification

---

## 📚 **TÀI LIỆU HỌC LIỆU - ĐÁP ÁN**

### ✅ **CÓ CẦN tài liệu PDF/Word/PPT**

**Lý do:**
1. **Học sinh cần ôn tập lý thuyết** trước khi làm bài tập/thi
2. **Giảng viên upload tài liệu tham khảo** (sách, slide, tóm tắt)
3. **Tăng value** của platform (không chỉ có câu hỏi)

**Implementation:**
```csharp
// Lesson.cs - ĐÃ CÓ SẴN
public string Content { get; set; } = string.Empty; // Nội dung HTML
public string Images { get; set; } = string.Empty; // JSON URLs
public string Videos { get; set; } = string.Empty; // JSON URLs
public string References { get; set; } = string.Empty; // JSON URLs

// THÊM MỚI (nếu cần):
public string Documents { get; set; } = string.Empty; // JSON:
// [
//   {"type": "pdf", "name": "Lý thuyết chương 1.pdf", "url": "..."},
//   {"type": "word", "name": "Tóm tắt.docx", "url": "..."},
//   {"type": "ppt", "name": "Slide bài giảng.pptx", "url": "..."}
// ]
```

**Storage Solution:**
- Azure Blob Storage (recommend) hoặc AWS S3
- CDN cho serving nhanh
- Access control (chỉ học sinh đã đăng ký)

**File types support:**
- ✅ PDF (lý thuyết, sách tham khảo)
- ✅ Word (.docx) - tóm tắt kiến thức
- ✅ PowerPoint (.pptx) - slide bài giảng
- ⚠️ Video: Upload to YouTube/Vimeo, embed link (đỡ tốn storage)

**Priority:** **MEDIUM** - Có tốt nhưng không phải ngay từ MVP

---

## 🎯 **ROADMAP ĐỀ XUẤT**

### **Sprint 1-2: MVP Core (4 tuần)**
1. Auth system ✅ (đã có)
2. Curriculum/Chapter/Lesson CRUD ✅ (entities đã có)
3. Question Bank basic (CRUD, phân loại)
4. Exam creation + Student attempt
5. Auto-grading

### **Sprint 3-4: AI & Premium Features (4 tuần)**
6. AI Question Generation (GPT-4 + Gemini)
7. Offline PWA support
8. Export Word (giáo án)
9. Upload tài liệu (PDF/Word)

### **Sprint 5-6: Polish & Scale (4 tuần)**
10. Practice system
11. Analytics dashboard
12. Performance optimization
13. Testing & Bug fixes

### **Sprint 7+: Optional (nếu còn thời gian)**
14. Subscription/Payment
15. Mobile app
16. Forum

---

## 📊 **TECHNOLOGY STACK - FINAL**

```yaml
Backend:
  - ASP.NET Core 8 Web API
  - EF Core (SQL Server)
  - SignalR (real-time)
  - Hangfire (background jobs)
  
AI:
  - OpenAI GPT-4 API
  - Google Gemini API
  
Frontend:
  - React + TypeScript
  - TailwindCSS
  - PWA (Service Worker)
  - IndexedDB (offline)
  
Storage:
  - Azure Blob Storage (files)
  - Redis (cache)
  
Export:
  - Open XML SDK (Word .docx)
  - iTextSharp (PDF)
  
Deployment:
  - Azure App Service / AWS
  - GitHub Actions (CI/CD)
```

---

## 🔑 **KEY SUCCESS FACTORS**

1. ✅ **Ngân hàng câu hỏi phong phú** (1000+ câu cho mỗi lớp)
2. ✅ **AI generation chất lượng cao** (giảng viên review & approve)
3. ✅ **Offline PWA hoạt động tốt** (không bị mất bài khi mất mạng)
4. ✅ **UI/UX thân thiện** (teacher & student đều dễ dùng)
5. ✅ **Performance** (load nhanh, responsive)

---

**Version:** 1.0  
**Last Updated:** 2024-11-14  
**Dự án:** Chemistry Subject - Lesson Planning & E-Learning Platform

