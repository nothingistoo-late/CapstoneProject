# API Response Messages By Module (VI)

Nguon: cac chuoi message trong Result.Success/Result.Failure tu source code hien tai.

## API/Cms

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Cần có tập tin Avatar. | N/A | POST /api/cms/maps |
| 2 | Đã hủy kích hoạt thành công {deletedCount} người dùng QuickLogin không hoạt động | N/A | POST /api/cms/users/cleanup-inactive-quick-login |
| 3 | Đã xảy ra lỗi trong quá trình dọn dẹp | N/A | POST /api/cms/users/cleanup-inactive-quick-login |
| 4 | Đầu vào tệp bản đồ không hợp lệ. | N/A | POST /api/cms/maps |
| 5 | Trường 'dữ liệu' (JSON) là bắt buộc. | N/A | POST /api/cms/maps |
| 6 | Trường 'dữ liệu' không phải là JSON hợp lệ. | N/A | POST /api/cms/maps |
| 7 | Trường 'dữ liệu' không thể được giải tuần tự hóa. | N/A | POST /api/cms/maps |

## API/Learner

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Cần có tập tin Avatar. | N/A | POST /api/learner/maps |
| 2 | Đầu vào tệp bản đồ không hợp lệ. | N/A | POST /api/learner/maps |
| 3 | Không thể giải tuần tự hóa trường 'dữ liệu' thành CreateMapRequest. | N/A | POST /api/learner/maps |
| 4 | Nội dung yêu cầu là bắt buộc | N/A | POST /api/learner/auth/google-login |
| 5 | Trường 'dữ liệu' (chuỗi JSON, giống như nội dung CreateMapRequest) là bắt buộc. | N/A | POST /api/learner/maps |
| 6 | Trường 'dữ liệu' không phải là JSON hợp lệ. | N/A | POST /api/learner/maps |

## API/Other

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn chưa được xác thực. | N/A | Global auth filter (áp dụng cho endpoint yêu cầu quyền) |
| 2 | Bạn không có quyền truy cập vào {systemName}. | N/A | Global role filter (áp dụng cho endpoint phân quyền) |
| 3 | Chỉ có {allowedRoles} mới có thể truy cập khu vực này. Vui lòng sử dụng một tài khoản thích hợp. | N/A | Global role filter (áp dụng cho endpoint phân quyền) |
| 4 | Dữ liệu đầu vào không hợp lệ | N/A | Global validation middleware (áp dụng cho tất cả endpoint) |

## Application/Auth

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã bắt đầu đặt lại mật khẩu. Vui lòng xác minh OTP được gửi tới {command.Request.OtpSentChannel.ToString().ToLower()} của bạn để hoàn tất quy trình đặt lại mật khẩu. | N/A | POST /api/learner/auth/reset-password |
| 2 | Đã thay đổi mật khẩu thành công | N/A | POST /api/learner/auth/change-password |
| 3 | Đã xảy ra lỗi khi cập nhật hồ sơ | N/A | PUT /api/cms/auth/profile ; PUT /api/learner/auth/profile |
| 4 | Đã xảy ra lỗi khi đăng nhập | N/A | POST /api/cms/auth/login ; POST /api/learner/auth/login |
| 5 | Đã xảy ra lỗi khi đăng nhập nhanh | N/A | POST /api/learner/auth/quick-login |
| 6 | Đã xảy ra lỗi khi đăng xuất | N/A | POST /api/cms/auth/logout ; POST /api/learner/auth/logout |
| 7 | Đã xảy ra lỗi khi truy xuất hồ sơ | N/A | GET /api/cms/auth/profile ; GET /api/learner/auth/profile |
| 8 | Đã xảy ra lỗi khi xác minh OTP. | N/A | POST /api/learner/auth/verify-otp |
| 9 | Đăng ký bắt đầu. Vui lòng xác minh OTP được gửi tới {channel.ToString().ToLower()} của bạn để hoàn tất quá trình đăng ký. | N/A | POST /api/learner/auth/register |
| 10 | Đăng nhập bằng Google thành công. | N/A | POST /api/learner/auth/google |
| 11 | Đăng nhập nhanh không có sẵn | N/A | POST /api/learner/auth/quick-login |
| 12 | Đăng nhập nhanh tạm thời bị vô hiệu hóa. | N/A | POST /api/learner/auth/quick-login |
| 13 | Đăng nhập nhanh thành công! | N/A | POST /api/learner/auth/quick-login |
| 14 | Đăng nhập thành công! | N/A | POST /api/cms/auth/login ; POST /api/learner/auth/login |
| 15 | Đăng xuất thành công! | N/A | POST /api/cms/auth/logout ; POST /api/learner/auth/logout |
| 16 | Đặt lại mật khẩu thành công. | N/A | POST /api/learner/auth/verify-otp |
| 17 | Dữ liệu người dùng bị thiếu sau khi xác minh OTP. | N/A | POST /api/learner/auth/verify-otp |
| 18 | Hồ sơ được cập nhật thành công | N/A | PUT /api/cms/auth/profile ; PUT /api/learner/auth/profile |
| 19 | Hồ sơ được truy xuất thành công | N/A | GET /api/cms/auth/profile ; GET /api/learner/auth/profile |
| 20 | IdToken là bắt buộc. | N/A | POST /api/learner/auth/google |
| 21 | Không được ủy quyền | N/A | POST /api/learner/auth/change-password |
| 22 | Không gửi được mã xác minh. Vui lòng thử lại. | N/A | POST /api/learner/auth/register |
| 23 | Không tạo được người dùng | N/A | POST /api/learner/auth/quick-login |
| 24 | Không tạo được người dùng từ Google. | N/A | POST /api/learner/auth/google |
| 25 | Không thể cập nhật hồ sơ | N/A | PUT /api/cms/auth/profile ; PUT /api/learner/auth/profile |
| 26 | Không thể cập nhật người dùng | N/A | POST /api/learner/auth/verify-otp |
| 27 | Không thể chỉ định vai trò. | N/A | POST /api/learner/auth/google |
| 28 | Không thể thêm người dùng vào vai trò | N/A | POST /api/learner/auth/quick-login |
| 29 | Không tìm thấy email tài khoản Google. | N/A | POST /api/learner/auth/google |
| 30 | Không tìm thấy người dùng | N/A | POST /api/cms/auth/logout ; POST /api/learner/auth/logout |
| 31 | Không tìm thấy người dùng sau khi đăng nhập Google. | N/A | POST /api/learner/auth/google |
| 32 | Làm mới mã thông báo thành công! | N/A | POST /api/cms/auth/refresh-token ; POST /api/learner/auth/refresh-token |
| 33 | Loại OTP không hợp lệ. | N/A | POST /api/learner/auth/verify-otp |
| 34 | Lỗi đổi mật khẩu | N/A | POST /api/learner/auth/change-password |
| 35 | Lỗi làm mới mã thông báo | N/A | POST /api/cms/auth/refresh-token ; POST /api/learner/auth/refresh-token |
| 36 | Lỗi trong quá trình đăng ký | N/A | POST /api/learner/auth/register |
| 37 | Lỗi trong quá trình đặt lại mật khẩu | N/A | POST /api/learner/auth/reset-password |
| 38 | Mã nhanh không hợp lệ | N/A | POST /api/learner/auth/quick-login |
| 39 | Mã thông báo Google không hợp lệ. | N/A | POST /api/learner/auth/google |
| 40 | Mật khẩu hiện tại không chính xác | N/A | POST /api/learner/auth/change-password |
| 41 | Người dùng chưa được xác thực | N/A | PUT /api/cms/auth/profile ; PUT /api/learner/auth/profile |
| 42 | Người dùng đã đăng ký thành công. | N/A | POST /api/learner/auth/verify-otp |
| 43 | Số điện thoại đã tồn tại | N/A | PUT /api/cms/auth/profile ; PUT /api/learner/auth/profile |
| 44 | Thông tin liên hệ là bắt buộc | N/A | POST /api/learner/auth/register |
| 45 | Thông tin xác thực không hợp lệ | N/A | POST /api/cms/auth/login ; POST /api/learner/auth/login |

## Application/Chat

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn chỉ có thể chỉnh sửa tin nhắn của riêng bạn | N/A | PUT /api/learner/chat/messages/{messageId} |
| 2 | Bạn chỉ có thể xóa tin nhắn của riêng bạn | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 3 | Bạn không phải là người tham gia vào cuộc trò chuyện này | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 4 | Cần có ID người dùng khác | N/A | POST /api/learner/chat/conversations/private |
| 5 | Cần có ID phòng trò chuyện | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 6 | Đã cập nhật tin nhắn. | N/A | PUT /api/learner/chat/messages/{messageId} |
| 7 | Đã đóng cuộc trò chuyện. | N/A | POST /api/learner/chat/conversations/{conversationId}/close |
| 8 | Đã gửi tin nhắn. | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 9 | Đã tạo cuộc trò chuyện nhóm tạm thời. | N/A | POST /api/learner/chat/conversations/temporary-group |
| 10 | Đã tạo hoặc lấy cuộc trò chuyện riêng tư. | N/A | POST /api/learner/chat/conversations/private |
| 11 | Đã thêm thành viên vào cuộc trò chuyện. | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 12 | Đã xảy ra lỗi không mong muốn khi cập nhật tin nhắn | N/A | PUT /api/learner/chat/messages/{messageId} |
| 13 | Đã xảy ra lỗi không mong muốn khi gửi tin nhắn | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 14 | Đã xảy ra lỗi không mong muốn khi kết thúc cuộc trò chuyện | N/A | POST /api/learner/chat/conversations/{conversationId}/close |
| 15 | Đã xảy ra lỗi không mong muốn khi tạo cuộc trò chuyện | N/A | POST /api/learner/chat/conversations/private |
| 16 | Đã xảy ra lỗi không mong muốn khi thêm thành viên | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 17 | Đã xảy ra lỗi không mong muốn khi xóa tin nhắn | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 18 | Đã xóa tin nhắn. | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 19 | ID cuộc trò chuyện là bắt buộc | N/A | POST /api/learner/chat/conversations/{conversationId}/close |
| 20 | ID người dùng là bắt buộc | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 21 | ID tin nhắn là bắt buộc | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 22 | Không cập nhật được tin nhắn do lỗi cơ sở dữ liệu | N/A | PUT /api/learner/chat/messages/{messageId} |
| 23 | Không gửi được tin nhắn do lỗi cơ sở dữ liệu | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 24 | Không lấy được thông tin thành viên | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 25 | Không tạo được cuộc trò chuyện do lỗi cơ sở dữ liệu | N/A | POST /api/learner/chat/conversations/private |
| 26 | Không tạo được tin nhắn | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 27 | Không thể chỉnh sửa tin nhắn đã xóa | N/A | PUT /api/learner/chat/messages/{messageId} |
| 28 | Không thể chỉnh sửa tin nhắn trong cuộc trò chuyện đã đóng | N/A | PUT /api/learner/chat/messages/{messageId} |
| 29 | Không thể đóng cuộc trò chuyện do lỗi cơ sở dữ liệu | N/A | POST /api/learner/chat/conversations/{conversationId}/close |
| 30 | Không thể gửi tin nhắn đến cuộc trò chuyện đã đóng | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 31 | Không thể tạo cuộc trò chuyện với chính mình | N/A | POST /api/learner/chat/conversations/private |
| 32 | Không thể thêm thành viên do lỗi cơ sở dữ liệu | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 33 | Không tìm thấy cuộc trò chuyện | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 34 | Không tìm thấy tin nhắn | N/A | PUT /api/learner/chat/messages/{messageId} |
| 35 | Không tìm thấy tin nhắn hoặc đã bị xóa | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 36 | Không xóa được tin nhắn do lỗi cơ sở dữ liệu | N/A | DELETE /api/learner/chat/messages/{messageId} |
| 37 | Lệnh không thể rỗng | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 38 | Người dùng chưa được xác thực | N/A | SignalR /hubs/chat (không có endpoint REST công khai) |
| 39 | Nội dung tin nhắn không được để trống | N/A | PUT /api/learner/chat/messages/{messageId} |
| 40 | Nội dung tin nhắn không được vượt quá 5000 ký tự | N/A | PUT /api/learner/chat/messages/{messageId} |
| 41 | Nội dung tin nhắn là bắt buộc đối với tin nhắn văn bản | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |
| 42 | Tên nhóm là bắt buộc | N/A | POST /api/learner/chat/conversations/temporary-group |
| 43 | Yêu cầu không thể rỗng | N/A | POST /api/learner/chat/conversations/{conversationId}/messages |

## Application/Community

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn chỉ có thể báo cáo những bản đồ mà bạn có quyền truy cập (bản đồ miễn phí hoặc bản đồ bạn đã mua). | N/A | POST /api/learner/community/maps/{mapId:guid}/report |
| 2 | Bạn chỉ có thể xếp hạng bản đồ mà bạn có quyền truy cập (bản đồ miễn phí hoặc bản đồ bạn đã mua). | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 3 | Bạn không có quyền giải quyết các báo cáo. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện giải quyết hàng loạt. | N/A | POST /api/cms/community/reports/batch/resolve |
| 4 | Bạn không có quyền giải quyết các báo cáo. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/community/reports/{reportId:guid}/resolve |
| 5 | Bạn không có quyền loại bỏ báo cáo. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/community/reports/{reportId:guid}/dismiss |
| 6 | Bạn không có quyền loại bỏ báo cáo. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện loại bỏ hàng loạt. | N/A | POST /api/cms/community/reports/batch/dismiss |
| 7 | Bạn không thể báo cáo bản đồ của riêng bạn. | N/A | POST /api/learner/community/maps/{mapId:guid}/report |
| 8 | Bạn không thể xếp hạng bản đồ của riêng bạn. | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 9 | Báo cáo bị loại bỏ. | N/A | POST /api/cms/community/reports/{reportId:guid}/dismiss |
| 10 | Báo cáo đã được giải quyết. | N/A | POST /api/cms/community/reports/{reportId:guid}/resolve |
| 11 | Đã giải quyết (các) báo cáo {dto.SuccessCount}. | N/A | POST /api/cms/community/reports/batch/resolve |
| 12 | Đã gửi báo cáo. | N/A | POST /api/learner/community/maps/{mapId:guid}/report |
| 13 | Đã loại bỏ (các) báo cáo {dto.SuccessCount}. | N/A | POST /api/cms/community/reports/batch/dismiss |
| 14 | Đã lưu xếp hạng. | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 15 | Đánh giá phải từ 1 đến 5 sao. Vui lòng cung cấp đánh giá hợp lệ. | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 16 | Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại. | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 17 | Không tìm thấy báo cáo có Id: {command.ReportId}. Báo cáo có thể đã bị xóa hoặc không tồn tại. | N/A | POST /api/cms/community/reports/{reportId:guid}/dismiss |
| 18 | Lý do báo cáo là bắt buộc. Vui lòng cung cấp lý do để báo cáo nội dung này. | N/A | POST /api/learner/community/maps/{mapId:guid}/report |
| 19 | Yêu cầu xác thực. Vui lòng đăng nhập để báo cáo bản đồ. | N/A | POST /api/learner/community/maps/{mapId:guid}/report |
| 20 | Yêu cầu xác thực. Vui lòng đăng nhập để đánh giá bản đồ. | N/A | POST /api/learner/community/maps/{mapId:guid}/rate |
| 21 | Yêu cầu xác thực. Vui lòng đăng nhập để giải quyết báo cáo. | N/A | POST /api/cms/community/reports/{reportId:guid}/resolve |
| 22 | Yêu cầu xác thực. Vui lòng đăng nhập để giải quyết các báo cáo. | N/A | POST /api/cms/community/reports/batch/resolve |
| 23 | Yêu cầu xác thực. Vui lòng đăng nhập để loại bỏ báo cáo. | N/A | POST /api/cms/community/reports/batch/dismiss |

## Application/Competitive

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã tạo trận đấu. | N/A | POST /api/learner/competitive/matches |
| 2 | Không tìm thấy bản đồ | N/A | POST /api/learner/competitive/matches |
| 3 | Không tìm thấy phòng | N/A | POST /api/learner/competitive/rooms/join |
| 4 | Không tìm thấy trận đấu | N/A | POST /api/learner/competitive/matches/{matchId:guid}/rooms |
| 5 | Phòng đã đầy | N/A | POST /api/learner/competitive/rooms/join |
| 6 | Phòng không chờ | N/A | POST /api/learner/competitive/rooms/join |
| 7 | Yêu cầu xác thực. Vui lòng đăng nhập để tạo phòng. | N/A | POST /api/learner/competitive/matches/{matchId:guid}/rooms |
| 8 | Yêu cầu xác thực. Vui lòng đăng nhập để tạo trận đấu. | N/A | POST /api/learner/competitive/matches |
| 9 | Yêu cầu xác thực. Vui lòng đăng nhập để tham gia phòng. | N/A | POST /api/learner/competitive/rooms/join |

## Application/Complaints

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn không có quyền gửi tin nhắn cho khiếu nại này. | N/A | POST /api/learner/complaints/{complaintId:guid}/messages |
| 2 | Bạn không có quyền gửi tin nhắn cho nhân viên. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/complaints/{complaintId:guid}/messages |
| 3 | Bạn không có quyền thay đổi trạng thái khiếu nại. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 4 | Bạn không có quyền xem chi tiết khiếu nại. Chỉ có Quản trị viên hoặc Người điều hành mới có thể truy cập. | N/A | GET /api/cms/complaints/{complaintId:guid} |
| 5 | Bạn không có quyền xem khiếu nại này. | N/A | GET /api/learner/complaints/{complaintId:guid} |
| 6 | CategoryKey là bắt buộc. | N/A | POST /api/learner/complaints |
| 7 | CategoryKey và RuleKey là bắt buộc. | N/A | DELETE /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 8 | Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình danh mục khiếu nại. | N/A | PUT /api/cms/complaints/config/categories/{categoryKey} |
| 9 | Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình quy tắc chính sách khiếu nại. | N/A | PUT /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 10 | Chỉ Quản trị viên/Người điều hành mới có thể xóa cấu hình danh mục khiếu nại. | N/A | DELETE /api/cms/complaints/config/categories/{categoryKey} |
| 11 | Chỉ Quản trị viên/Người điều hành mới có thể xóa cấu hình quy tắc chính sách khiếu nại. | N/A | DELETE /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 12 | Chủ đề là bắt buộc. | N/A | POST /api/learner/complaints |
| 13 | Chuyển đổi trạng thái không hợp lệ: {fromStatus} -> {toStatus}. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 14 | Đã cập nhật trạng thái khiếu nại. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 15 | Đã gửi đơn khiếu nại. | N/A | POST /api/learner/complaints |
| 16 | Đã gửi tin nhắn. | N/A | POST /api/learner/complaints/{complaintId:guid}/messages |
| 17 | Đã lấy chi tiết khiếu nại của bạn. | N/A | GET /api/learner/complaints/{complaintId:guid} |
| 18 | Đã lấy chi tiết khiếu nại. | N/A | GET /api/cms/complaints/{complaintId:guid} |
| 19 | Đã lưu cấu hình danh mục khiếu nại. | N/A | PUT /api/cms/complaints/config/categories/{categoryKey} |
| 20 | Đã lưu cấu hình quy tắc chính sách khiếu nại. | N/A | PUT /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 21 | Đã xóa cấu hình danh mục khiếu nại. | N/A | DELETE /api/cms/complaints/config/categories/{categoryKey} |
| 22 | Đã xóa cấu hình quy tắc chính sách khiếu nại. | N/A | DELETE /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 23 | Khiếu nại này đã được giải quyết. Bạn không thể gửi tin nhắn mới. | N/A | POST /api/learner/complaints/{complaintId:guid}/messages |
| 24 | Khiếu nạiId là bắt buộc. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 25 | Không có thay đổi trạng thái. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 26 | Không thể tải lên một hoặc nhiều tệp đính kèm. | N/A | POST /api/learner/complaints |
| 27 | Không tìm thấy cấu hình danh mục khiếu nại cho quy tắc này. | N/A | PUT /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 28 | Không tìm thấy cấu hình danh mục khiếu nại. | N/A | DELETE /api/cms/complaints/config/categories/{categoryKey} |
| 29 | Không tìm thấy cấu hình quy tắc chính sách khiếu nại. | N/A | DELETE /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 30 | Không tìm thấy khiếu nại với Id: {command.ComplaintId}. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 31 | Không tìm thấy khiếu nại với Id: {request.ComplaintId}. | N/A | GET /api/cms/complaints/{complaintId:guid} |
| 32 | Mô tả là bắt buộc. | N/A | POST /api/learner/complaints |
| 33 | Nội dung tin nhắn là bắt buộc. | N/A | POST /api/learner/complaints/{complaintId:guid}/messages |
| 34 | RuleKey là bắt buộc. | N/A | PUT /api/cms/complaints/config/rules/{categoryKey}/{ruleKey} |
| 35 | Tên hiển thị là bắt buộc. | N/A | PUT /api/cms/complaints/config/categories/{categoryKey} |
| 36 | Xác thực chính sách khiếu nại không thành công. | N/A | POST /api/learner/complaints |
| 37 | Yêu cầu xác thực. | N/A | DELETE /api/cms/complaints/config/categories/{categoryKey} |
| 38 | Yêu cầu xác thực. Vui lòng đăng nhập để gửi khiếu nại. | N/A | POST /api/learner/complaints |
| 39 | Yêu cầu xác thực. Vui lòng đăng nhập để gửi tin nhắn cho nhân viên. | N/A | POST /api/cms/complaints/{complaintId:guid}/messages |
| 40 | Yêu cầu xác thực. Vui lòng đăng nhập để gửi tin nhắn. | N/A | POST /api/learner/complaints/{complaintId:guid}/messages |
| 41 | Yêu cầu xác thực. Vui lòng đăng nhập để thay đổi trạng thái khiếu nại. | N/A | POST /api/cms/complaints/{complaintId:guid}/status |
| 42 | Yêu cầu xác thực. Vui lòng đăng nhập để xem chi tiết khiếu nại. | N/A | GET /api/cms/complaints/{complaintId:guid} |

## Application/Gameplay

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình điểm giải quyết bản đồ. | N/A | PUT /api/cms/gameplay/map-solve-score |
| 2 | Chỉ Quản trị viên/Người điều hành mới có thể xem cấu hình điểm giải quyết bản đồ. | N/A | GET /api/cms/gameplay/map-solve-score |
| 3 | Đã cập nhật cấu hình điểm giải quyết bản đồ. | N/A | PUT /api/cms/gameplay/map-solve-score |
| 4 | Đã chấm lời giải thành công. | N/A | POST /api/learner/gameplay/validate |
| 5 | Đã lấy bảng điều khiển tiến trình. | N/A | GET /api/learner/gameplay/dashboard |
| 6 | Đã truy xuất thành công | N/A | GET /api/learner/gameplay/my-play-history |
| 7 | Không cấp được lộ trình học tập XP. | N/A | POST /api/learner/gameplay/validate |
| 8 | Không cấp được XP. | N/A | POST /api/learner/gameplay/validate |
| 9 | Không còn lượt dùng thử miễn phí nào cho bản đồ này. | N/A | POST /api/learner/gameplay/validate |
| 10 | Không tìm thấy bản đồ | N/A | POST /api/learner/gameplay/validate |
| 11 | Không tìm thấy cấu hình điểm giải quyết bản đồ. | N/A | PUT /api/cms/gameplay/map-solve-score |
| 12 | Không tìm thấy dữ liệu bản đồ | N/A | POST /api/learner/gameplay/validate |
| 13 | Không tìm thấy người dùng. | N/A | GET /api/learner/gameplay/dashboard |
| 14 | MapDetailId là bắt buộc khi bản đồ có nhiều cấp độ hoặc không hợp lệ đối với bản đồ này. | N/A | POST /api/learner/gameplay/validate |
| 15 | Yêu cầu xác thực. | N/A | PUT /api/cms/gameplay/map-solve-score |
| 16 | Yêu cầu xác thực. Vui lòng đăng nhập để xác nhận giải pháp. | N/A | POST /api/learner/gameplay/validate |
| 17 | Yêu cầu xác thực. Vui lòng đăng nhập để xem bảng điều khiển tiến trình của bạn. | N/A | GET /api/learner/gameplay/dashboard |

## Application/LearningPath

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã lấy chi tiết khái niệm. | N/A | GET /api/learner/learning-path/concepts/{conceptId:guid} |
| 2 | Đã lấy chi tiết mục tiêu học tập. | N/A | GET /api/learner/learning-path/goals/{goalId:guid} |
| 3 | Đã lấy lộ trình học tập của bạn. | N/A | GET /api/learner/learning-path/my-path |
| 4 | Đã lấy mục tiêu học tập đã chọn. | N/A | GET /api/learner/learning-path/my-path/selected-goal |
| 5 | Đã lấy tiến độ lộ trình học tập của bạn. | N/A | GET /api/learner/learning-path/my-path/progress |
| 6 | Đã lấy trạng thái hoàn thành khái niệm. | N/A | GET /api/learner/learning-path/concepts/{conceptId:guid}/completion |
| 7 | Khái niệm đã hoàn thành. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 8 | Khái niệm đã hoàn thành. Mục tiếp theo trong đường dẫn của bạn hiện đã được mở khóa. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 9 | Khái niệm không được tìm thấy. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 10 | Không cấp được khái niệm XP. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 11 | Không cấp được lộ trình học tập XP. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 12 | Không tìm thấy mục tiêu học tập. | N/A | POST /api/learner/learning-path/goals/select |
| 13 | Mục tiêu học tập đã chọn. Đường dẫn của bạn đã được cập nhật. | N/A | POST /api/learner/learning-path/goals/select |
| 14 | Yêu cầu xác thực. | N/A | GET /api/learner/learning-path/concepts/{conceptId:guid}/completion |
| 15 | Yêu cầu xác thực. Vui lòng đăng nhập để chọn mục tiêu học tập. | N/A | POST /api/learner/learning-path/goals/select |
| 16 | Yêu cầu xác thực. Vui lòng đăng nhập để hoàn thành một khái niệm. | N/A | POST /api/learner/learning-path/concepts/{conceptId:guid}/complete |
| 17 | Yêu cầu xác thực. Vui lòng đăng nhập để xem lộ trình học tập của bạn. | N/A | GET /api/learner/learning-path/my-path |

## Application/Lobby

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bản đồ được cập nhật. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/map |
| 2 | Bản đồ không được tìm thấy hoặc đã bị xóa. | N/A | POST /api/learner/lobby/rooms |
| 3 | Bản đồ không được tìm thấy hoặc đã bị xóa. Chọn bản đồ khác. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/start |
| 4 | Bạn không ở trong phòng này. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 5 | Cung cấp RoomId hoặc RoomCode. | N/A | POST /api/learner/lobby/rooms/join |
| 6 | Đã cập nhật trạng thái sẵn sàng. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/ready |
| 7 | Đã ghi lại nội dung gửi. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 8 | Đã lấy thông tin phòng. | N/A | GET /api/learner/lobby/rooms/{roomId:guid} ; POST /api/learner/lobby/rooms/{roomId:guid}/map |
| 9 | Đã tham gia phòng. | N/A | POST /api/learner/lobby/rooms/join |
| 10 | Không tạo được phòng. | N/A | POST /api/learner/lobby/rooms |
| 11 | Không thể bắt đầu trò chơi. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/start |
| 12 | Không thể chuyển đổi sẵn sàng. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/ready |
| 13 | Không thể ghi lại bài nộp. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 14 | Không thể kết thúc trò chơi. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/end |
| 15 | Không thể rời khỏi phòng. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/leave |
| 16 | Không thể tạo phòng. Bạn đã ở trong một phòng rồi. Vui lòng rời phòng hiện tại trước khi tạo phòng mới. | N/A | POST /api/learner/lobby/rooms |
| 17 | Không thể tham gia. | N/A | POST /api/learner/lobby/rooms/join |
| 18 | Không thể thiết lập bản đồ. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/map |
| 19 | Không tìm thấy phòng. | N/A | POST /api/learner/lobby/rooms/join |
| 20 | Phòng bên trái. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/leave |
| 21 | Phòng chưa có bản đồ nào được chọn. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 22 | Phòng được tạo. | N/A | POST /api/learner/lobby/rooms |
| 23 | Trò chơi bắt đầu. Kết nối với SignalR để nhận thông tin cập nhật theo thời gian thực. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/start |
| 24 | Trò chơi kết thúc. Phòng đang chờ lần khởi động tiếp theo. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/end |
| 25 | Trò chơi không được tiến hành. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 26 | Xác thực không thành công. | N/A | POST /api/learner/lobby/rooms/{roomId:guid}/submit |
| 27 | Yêu cầu xác thực. | N/A | POST /api/learner/lobby/rooms |

## Application/Maps

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | (Các) bản đồ {dto.SuccessCount} đã được phê duyệt. | N/A | POST /api/cms/maps/batch/approve |
| 2 | Bạn chỉ có thể gửi bản đồ của riêng mình để xem xét. Bản đồ này được tạo bởi một người dùng khác. | N/A | POST /api/learner/maps/{id:guid}/submit |
| 3 | Bản đồ đã bị từ chối thành công. | N/A | POST /api/cms/maps/{id:guid}/reject |
| 4 | Bản đồ đã có trong bộ sưu tập của bạn. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 5 | Bản đồ đã được cập nhật và chuyển về trạng thái Bản nháp. | N/A | PUT /api/learner/maps/{id:guid} |
| 6 | Bản đồ đã được gửi để xem xét thành công. | N/A | POST /api/learner/maps/{id:guid}/submit |
| 7 | Bản đồ đã được phê duyệt thành công. | N/A | POST /api/cms/maps/{id:guid}/approve |
| 8 | Bản đồ đã được xóa thành công. | N/A | DELETE /api/cms/maps/{id:guid} ; DELETE /api/learner/maps/{id:guid} |
| 9 | Bản đồ được xuất bản thành công. | N/A | POST /api/cms/maps/{id:guid}/publish ; POST /api/learner/maps/{id:guid}/publish |
| 10 | Bản đồ không được tìm thấy hoặc đã bị xóa. | N/A | SignalR /hubs/gamelobby (CreateRoom/SetSelectedMap/StartGame) |
| 11 | Bản đồ không được tìm thấy hoặc không hoạt động. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 12 | Bản đồ không được tìm thấy. | N/A | DELETE /api/cms/maps/{id:guid}/gallery/{mediaId:guid} ; DELETE /api/learner/maps/{id:guid}/gallery/{mediaId:guid} |
| 13 | Bản đồ không thể bị từ chối. Trạng thái dự kiến: Đang chờ xem xét. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ đang chờ xem xét mới có thể bị từ chối. | N/A | POST /api/cms/maps/{id:guid}/reject |
| 14 | Bản đồ không thể được gửi để xem xét. Trạng thái dự kiến: Bản nháp. Trạng thái hiện tại: {map.MapStatus}. Chỉ có thể gửi bản đồ dự thảo. | N/A | POST /api/learner/maps/{id:guid}/submit |
| 15 | Bản đồ không thể được phê duyệt. Trạng thái dự kiến: Đang chờ xem xét. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ đang chờ xem xét mới có thể được phê duyệt. | N/A | POST /api/cms/maps/{id:guid}/approve |
| 16 | Bản đồ không thể được xuất bản. Trạng thái dự kiến: Đã phê duyệt. Trạng thái hiện tại: {map.MapStatus}. Chỉ những bản đồ được phê duyệt mới có thể được xuất bản. | N/A | POST /api/cms/maps/{id:guid}/publish ; POST /api/learner/maps/{id:guid}/publish |
| 17 | Bản đồ miễn phí được thêm vào bộ sưu tập của bạn. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 18 | Bản đồ nguồn không có cấp độ để sao chép. | N/A | POST /api/cms/maps/{id:guid}/duplicate-as-new ; POST /api/learner/maps/{id:guid}/duplicate-as-new |
| 19 | Bản đồ tồn tại. | N/A | SignalR /hubs/gamelobby (CreateRoom/SetSelectedMap/StartGame) |
| 20 | Bạn không có quyền cập nhật bản đồ này. | N/A | DELETE /api/cms/maps/{id:guid}/gallery/{mediaId:guid} ; DELETE /api/learner/maps/{id:guid}/gallery/{mediaId:guid} |
| 21 | Bạn không có quyền cập nhật hình đại diện của bản đồ này. | N/A | POST /api/cms/maps/{id:guid}/avatar ; POST /api/learner/maps/{id:guid}/avatar |
| 22 | Bạn không có quyền cập nhật thẻ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | PUT /api/cms/maps/tags/{id:guid} |
| 23 | Bạn không có quyền phê duyệt bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/maps/{id:guid}/approve |
| 24 | Bạn không có quyền phê duyệt bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện phê duyệt hàng loạt. | N/A | POST /api/cms/maps/batch/approve |
| 25 | Bạn không có quyền tạo thẻ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/maps/tags |
| 26 | Bạn không có quyền từ chối bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | POST /api/cms/maps/{id:guid}/reject |
| 27 | Bạn không có quyền từ chối bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện từ chối hàng loạt. | N/A | POST /api/cms/maps/batch/reject |
| 28 | Bạn không có quyền xóa bản đồ này. Chỉ tác giả bản đồ hoặc Quản trị viên/Người điều hành mới có thể xóa nó. | N/A | DELETE /api/cms/maps/{id:guid} ; DELETE /api/learner/maps/{id:guid} |
| 29 | Bạn không có quyền xóa thẻ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện hành động này. | N/A | DELETE /api/cms/maps/tags/{id:guid} |
| 30 | Bạn không có quyền xuất bản bản đồ. | N/A | POST /api/cms/maps/{id:guid}/publish ; POST /api/learner/maps/{id:guid}/publish |
| 31 | Bạn không có quyền xuất bản bản đồ. Chỉ Quản trị viên hoặc Người điều hành mới có thể thực hiện xuất bản hàng loạt. | N/A | POST /api/cms/maps/batch/publish |
| 32 | Bạn không được phép sao chép bản đồ này. | N/A | POST /api/cms/maps/{id:guid}/duplicate-as-new ; POST /api/learner/maps/{id:guid}/duplicate-as-new |
| 33 | Cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong JSON hoặc Levels khi sử dụng API nhiều cấp). | N/A | PUT /api/learner/maps/{id:guid} |
| 34 | Chỉ có thể thêm bản đồ miễn phí vào bộ sưu tập của bạn. Bản đồ này được trả tiền. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 35 | Chỉ những bản đồ miễn phí đã xuất bản mới có thể được thêm vào bộ sưu tập của bạn. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 36 | Chỉ tác giả của bản đồ này mới có thể xuất bản nó. | N/A | POST /api/cms/maps/{id:guid}/publish ; POST /api/learner/maps/{id:guid}/publish |
| 37 | Đã cập nhật hình đại diện bản đồ. | N/A | POST /api/cms/maps/{id:guid}/avatar ; POST /api/learner/maps/{id:guid}/avatar |
| 38 | Đã cập nhật thẻ. | N/A | PUT /api/cms/maps/tags/{id:guid} |
| 39 | Đã kiểm tra quyền sở hữu bản đồ. | N/A | GET /api/learner/maps/{id:guid}/check-ownership |
| 40 | Đã lấy chi tiết bản đồ. | N/A | GET /api/cms/maps/{id:guid} ; GET /api/learner/maps/{id:guid} |
| 41 | Đã lấy thông tin bản đồ. | N/A | GET /api/learner/maps/{id:guid}/info |
| 42 | Đã tạo thẻ. | N/A | POST /api/cms/maps/tags |
| 43 | Đã truy xuất thành công | N/A | GET /api/cms/maps/all |
| 44 | Đã từ chối (các) bản đồ {dto.SuccessCount}. | N/A | POST /api/cms/maps/batch/reject |
| 45 | Đã xóa thẻ. | N/A | DELETE /api/cms/maps/tags/{id:guid} |
| 46 | Đã xuất bản {dto.SuccessCount} bản đồ. | N/A | POST /api/cms/maps/batch/publish |
| 47 | Id bản đồ là bắt buộc. | N/A | SignalR /hubs/gamelobby (CreateRoom/SetSelectedMap/StartGame) |
| 48 | Không tìm thấy bản đồ có Id: {command.MapId}. | N/A | PUT /api/learner/maps/{id:guid} |
| 49 | Không tìm thấy bản đồ có Id: {command.MapId}. Bản đồ có thể đã bị xóa hoặc không tồn tại. | N/A | POST /api/cms/maps/{id:guid}/approve |
| 50 | Không tìm thấy bản đồ có Id: {command.SourceMapId}. | N/A | POST /api/cms/maps/{id:guid}/duplicate-as-new ; POST /api/learner/maps/{id:guid}/duplicate-as-new |
| 51 | Không tìm thấy bản đồ có Id: {request.MapId}. | N/A | GET /api/cms/maps/{id:guid} ; GET /api/learner/maps/{id:guid} |
| 52 | Không tìm thấy mục thư viện. | N/A | DELETE /api/cms/maps/{id:guid}/gallery/{mediaId:guid} ; DELETE /api/learner/maps/{id:guid}/gallery/{mediaId:guid} |
| 53 | Không tìm thấy thẻ có Id: {command.TagId}. Thẻ có thể đã bị xóa hoặc không tồn tại. | N/A | DELETE /api/cms/maps/tags/{id:guid} |
| 54 | LearnedTagsCsv chứa (các) Hướng dẫn không hợp lệ. | N/A | POST /api/cms/maps/upload-json ; POST /api/learner/maps/upload-json |
| 55 | Mỗi cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong Levels[] hoặc timeLimitMs / winCondition trong JSON của mỗi cấp độ). | N/A | POST /api/cms/maps ; POST /api/cms/maps/with-files ; POST /api/learner/maps ; POST /api/learner/maps/with-files |
| 56 | Mỗi cấp độ yêu cầu TimeLimitMs và WinCondition > 0 (được đặt trong Levels[] hoặc trong JSON của mỗi cấp độ). | N/A | PUT /api/learner/maps/{id:guid} |
| 57 | Mục thư viện đã bị xóa. | N/A | DELETE /api/cms/maps/{id:guid}/gallery/{mediaId:guid} ; DELETE /api/learner/maps/{id:guid}/gallery/{mediaId:guid} |
| 58 | TagIdsCsv chứa (các) Hướng dẫn không hợp lệ. | N/A | POST /api/cms/maps/upload-json ; POST /api/learner/maps/upload-json |
| 59 | Tải lên hình đại diện không thành công. | N/A | POST /api/cms/maps/{id:guid}/avatar ; POST /api/learner/maps/{id:guid}/avatar |
| 60 | Tải lên thư viện không thành công. | N/A | POST /api/cms/maps ; POST /api/cms/maps/with-files ; POST /api/learner/maps ; POST /api/learner/maps/with-files |
| 61 | Tên thẻ là bắt buộc và không được để trống. | N/A | POST /api/cms/maps/tags |
| 62 | Tiêu đề không được vượt quá 200 ký tự. | N/A | POST /api/cms/maps/{id:guid}/duplicate-as-new ; POST /api/learner/maps/{id:guid}/duplicate-as-new |
| 63 | Yêu cầu xác thực. | N/A | DELETE /api/cms/maps/{id:guid}/gallery/{mediaId:guid} ; DELETE /api/learner/maps/{id:guid}/gallery/{mediaId:guid} |
| 64 | Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật bản đồ. | N/A | PUT /api/learner/maps/{id:guid} |
| 65 | Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật thẻ. | N/A | PUT /api/cms/maps/tags/{id:guid} |
| 66 | Yêu cầu xác thực. Vui lòng đăng nhập để gửi bản đồ để xem xét. | N/A | POST /api/learner/maps/{id:guid}/submit |
| 67 | Yêu cầu xác thực. Vui lòng đăng nhập để kiểm tra quyền sở hữu bản đồ. | N/A | GET /api/learner/maps/{id:guid}/check-ownership |
| 68 | Yêu cầu xác thực. Vui lòng đăng nhập để tạo bản đồ. | N/A | POST /api/cms/maps ; POST /api/cms/maps/with-files ; POST /api/learner/maps ; POST /api/learner/maps/with-files |
| 69 | Yêu cầu xác thực. Vui lòng đăng nhập để tạo thẻ. | N/A | POST /api/cms/maps/tags |
| 70 | Yêu cầu xác thực. Vui lòng đăng nhập để thêm bản đồ vào bộ sưu tập của bạn. | N/A | POST /api/learner/maps/{id:guid}/add-to-my-maps |
| 71 | Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện hành động này. | N/A | POST /api/cms/maps/{id:guid}/approve |
| 72 | Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện phê duyệt hàng loạt. | N/A | POST /api/cms/maps/batch/approve |
| 73 | Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện từ chối hàng loạt. | N/A | POST /api/cms/maps/batch/reject |
| 74 | Yêu cầu xác thực. Vui lòng đăng nhập để thực hiện xuất bản hàng loạt. | N/A | POST /api/cms/maps/batch/publish |
| 75 | Yêu cầu xác thực. Vui lòng đăng nhập để xóa bản đồ. | N/A | DELETE /api/cms/maps/{id:guid} ; DELETE /api/learner/maps/{id:guid} |
| 76 | Yêu cầu xác thực. Vui lòng đăng nhập để xóa thẻ. | N/A | DELETE /api/cms/maps/tags/{id:guid} |
| 77 | Yêu cầu xác thực. Vui lòng đăng nhập để xuất bản bản đồ. | N/A | POST /api/cms/maps/{id:guid}/publish ; POST /api/learner/maps/{id:guid}/publish |

## Application/Marketplace

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn không có quyền cập nhật gói. Chỉ Quản trị viên mới có thể thực hiện hành động này. | N/A | PUT /api/cms/marketplace/packages/{id:guid} |
| 2 | Bạn không có quyền cập nhật trạng thái gói. Chỉ Quản trị viên mới có thể thực hiện hành động này. | N/A | POST /api/cms/marketplace/packages/batch/status |
| 3 | Bạn không có quyền tạo gói. Chỉ Quản trị viên mới có thể thực hiện hành động này. | N/A | POST /api/cms/marketplace/packages |
| 4 | Bạn không có quyền xem báo cáo thanh toán. Chỉ quản trị viên mới có thể truy cập báo cáo này. | N/A | GET /api/cms/marketplace/reports/payments |
| 5 | Bạn không có quyền xóa các gói. Chỉ Quản trị viên mới có thể thực hiện hành động này. | N/A | DELETE /api/cms/marketplace/packages/{id:guid} |
| 6 | Đã cập nhật (các) gói {dto.SuccessCount}. | N/A | POST /api/cms/marketplace/packages/batch/status |
| 7 | Đã cập nhật gói. | N/A | PUT /api/cms/marketplace/packages/{id:guid} |
| 8 | Đã lấy thông tin gói. | N/A | GET /api/cms/marketplace/packages/{id:guid} ; GET /api/learner/marketplace/packages/{id:guid} |
| 9 | Đã tạo gói thành công. | N/A | POST /api/cms/marketplace/packages |
| 10 | Đã truy xuất thành công | N/A | GET /api/cms/marketplace/packages ; GET /api/learner/marketplace/packages |
| 11 | Đã xóa gói. | N/A | DELETE /api/cms/marketplace/packages/{id:guid} |
| 12 | Gói không được tìm thấy hoặc không hoạt động. | N/A | POST /api/learner/marketplace/packages/{id:guid}/purchase |
| 13 | Gói mua bằng OrbitCoin. | N/A | POST /api/learner/marketplace/packages/{id:guid}/purchase |
| 14 | Gói này không có giá; liên hệ hỗ trợ. | N/A | POST /api/learner/marketplace/packages/{id:guid}/purchase |
| 15 | Không tìm thấy gói có Id: {command.PackageId}. Gói có thể đã bị xóa hoặc không tồn tại. | N/A | DELETE /api/cms/marketplace/packages/{id:guid} |
| 16 | Không tìm thấy gói có Id: {request.PackageId}. Gói có thể đã bị xóa hoặc không tồn tại. | N/A | GET /api/cms/marketplace/packages/{id:guid} ; GET /api/learner/marketplace/packages/{id:guid} |
| 17 | OrbitCoin không đủ. Vui lòng nạp tiền trước. | N/A | POST /api/learner/marketplace/packages/{id:guid}/purchase |
| 18 | Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật một gói. | N/A | PUT /api/cms/marketplace/packages/{id:guid} |
| 19 | Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật trạng thái gói. | N/A | POST /api/cms/marketplace/packages/batch/status |
| 20 | Yêu cầu xác thực. Vui lòng đăng nhập để mua gói. | N/A | POST /api/learner/marketplace/packages/{id:guid}/purchase |
| 21 | Yêu cầu xác thực. Vui lòng đăng nhập để tạo gói. | N/A | POST /api/cms/marketplace/packages |
| 22 | Yêu cầu xác thực. Vui lòng đăng nhập để xem báo cáo thanh toán. | N/A | GET /api/cms/marketplace/reports/payments |
| 23 | Yêu cầu xác thực. Vui lòng đăng nhập để xóa một gói. | N/A | DELETE /api/cms/marketplace/packages/{id:guid} |

## Application/OrbitCoin

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bản đồ được mua bằng OrbitCoin. Phí nền tảng được khấu trừ từ người bán. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 2 | Bản đồ không có người tạo; không thể hoàn tất việc mua hàng. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 3 | Bản đồ không được tìm thấy. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 4 | Bản đồ này miễn phí và không thể mua bằng OrbitCoin. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 5 | Bạn không thể mua bản đồ của riêng bạn. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 6 | Chuyển hướng người dùng đến CheckoutUrl để hoàn tất thanh toán. | N/A | POST /api/learner/orbitcoin/deposit |
| 7 | Chuyển không thành công. | N/A | POST /api/learner/marketplace/maps/{mapId:guid}/purchase |
| 8 | Đã lấy lại lệnh gửi tiền. | N/A | GET /api/learner/orbitcoin/deposit/order |
| 9 | Đã lấy lại số dư. | N/A | GET /api/learner/orbitcoin/balance |
| 10 | Đã xác nhận tiền gửi. OrbitCoin đã được ghi có. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 11 | Dữ liệu đơn hàng không hợp lệ. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 12 | Hiệu quảTo phải lớn hơn hoặc bằng Hiệu quảTừ. | N/A | PUT /api/cms/orbitcoin/exchange-rate |
| 13 | Không thể tạo liên kết thanh toán. | N/A | POST /api/learner/orbitcoin/deposit |
| 14 | Không thể xác minh trạng thái thanh toán. Vui lòng thử lại hoặc liên hệ với bộ phận hỗ trợ. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 15 | Không tìm thấy đơn đặt hàng hoặc quyền truy cập bị từ chối. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 16 | Không tìm thấy tỷ giá hối đoái cho {request.FromCurrency}/{request.ToCurrency}. | N/A | GET /api/cms/orbitcoin/exchange-rate |
| 17 | Lịch sử giao dịch được truy xuất. | N/A | GET /api/learner/orbitcoin/transactions |
| 18 | OrbitCoin đã ghi có (tiền gửi được ghi lại). | N/A | POST /api/cms/orbitcoin/credit |
| 19 | Phương thức thanh toán PayOS chưa được định cấu hình. Liên hệ hỗ trợ. | N/A | POST /api/learner/orbitcoin/deposit |
| 20 | Số tiền phải dương. | N/A | POST /api/learner/orbitcoin/deposit |
| 21 | Số tiền quá nhỏ để chuyển đổi. | N/A | POST /api/learner/orbitcoin/deposit |
| 22 | Thanh toán chưa hoàn tất. Vui lòng chờ hoặc kiểm tra PayOS. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 23 | Tín dụng không thành công. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 24 | Tỷ giá hối đoái được cập nhật thành công. | N/A | PUT /api/cms/orbitcoin/exchange-rate |
| 25 | Tỷ giá hối đoái được truy xuất thành công. | N/A | GET /api/cms/orbitcoin/exchange-rate |
| 26 | Tỷ giá hối đoái phải dương. | N/A | PUT /api/cms/orbitcoin/exchange-rate |
| 27 | Việc gửi tiền đã hoàn tất. OrbitCoin đã được ghi có. | N/A | POST /api/learner/orbitcoin/deposit/confirm |
| 28 | Yêu cầu xác thực. | N/A | POST /api/learner/orbitcoin/deposit/confirm |

## Application/Other

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã xảy ra lỗi không mong muốn khi xử lý yêu cầu của bạn. Vui lòng thử lại sau. | N/A | Global exception handler (áp dụng cho tất cả endpoint) |

## Application/Recommendations

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã truy xuất thành công | N/A | GET /api/recommendations |
| 2 | Yêu cầu xác thực. | N/A | GET /api/recommendations |

## Application/User

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Bạn không có quyền cập nhật trạng thái người dùng. Chỉ Quản trị viên mới có thể thực hiện hành động này. | N/A | POST /api/cms/users/batch/status |
| 2 | Đã cập nhật trạng thái cho {dto.SuccessCount} người dùng. | N/A | POST /api/cms/users/batch/status |
| 3 | Không cập nhật được vai trò người dùng | N/A | PUT /api/cms/users/{id} |
| 4 | Không tạo được người dùng | N/A | POST /api/cms/users |
| 5 | Không thể cập nhật người dùng | N/A | PUT /api/cms/users/{id} |
| 6 | Không thể thêm người dùng vào vai trò | N/A | POST /api/cms/users |
| 7 | Không thể xóa người dùng | N/A | DELETE /api/cms/users/{id} |
| 8 | Người dùng đã cập nhật thành công | N/A | PUT /api/cms/users/{id} |
| 9 | Người dùng đã xóa thành công | N/A | DELETE /api/cms/users/{id} |
| 10 | Người dùng được tạo thành công | N/A | POST /api/cms/users |
| 11 | Nhận người dùng thành công | N/A | GET /api/cms/users/{id} |
| 12 | Yêu cầu xác thực. Vui lòng đăng nhập để cập nhật trạng thái người dùng. | N/A | POST /api/cms/users/batch/status |

## Application/Xp

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình chính sách XP. | N/A | PUT /api/cms/xp/config/policies/{policyKey} |
| 2 | Chỉ Quản trị viên/Người điều hành mới có thể cập nhật cấu hình nguồn XP. | N/A | PUT /api/cms/xp/config/sources/{sourceType} |
| 3 | Chỉ Quản trị viên/Người điều hành mới có thể cấp XP. | N/A | POST /api/cms/xp/grant |
| 4 | Chỉ Quản trị viên/Người điều hành mới có thể xem hồ sơ XP của người dùng. | N/A | GET /api/cms/xp/users/{userId:guid} |
| 5 | Đã cập nhật cấu hình chính sách XP. | N/A | PUT /api/cms/xp/config/policies/{policyKey} |
| 6 | Đã cập nhật cấu hình nguồn XP. | N/A | PUT /api/cms/xp/config/sources/{sourceType} |
| 7 | Không tìm thấy cấu hình chính sách cho khóa: {request.PolicyKey}. | N/A | PUT /api/cms/xp/config/policies/{policyKey} |
| 8 | Không tìm thấy cấu hình nguồn cho nguồn: {request.SourceType}. | N/A | PUT /api/cms/xp/config/sources/{sourceType} |
| 9 | Không tìm thấy người dùng. | N/A | GET /api/learner/xp/profile |
| 10 | Yêu cầu xác thực. | N/A | POST /api/cms/xp/grant |

## Infrastructure

| STT | Message (VI) | Message (EN) | API Endpoint |
|---:|---|---|
| 1 | Đã kiểm tra trùng email. | N/A | Shared auth service (nhiều endpoint Auth) |
| 2 | Đã kiểm tra trùng số điện thoại. | N/A | Shared auth service (nhiều endpoint Auth) |
| 3 | Email chưa được xác nhận | N/A | Shared auth service (nhiều endpoint Auth) |
| 4 | Email không hợp lệ | N/A | Shared auth service (nhiều endpoint Auth) |
| 5 | Id người dùng là bắt buộc. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 6 | IdempotencyKey là bắt buộc. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 7 | Không có XP được cấp sau khi đánh giá chính sách. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 8 | Không tìm thấy người dùng có Id: {input.UserId}. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 9 | Mật khẩu không hợp lệ | N/A | Shared auth service (nhiều endpoint Auth) |
| 10 | Người dùng không hoạt động | N/A | Shared auth service (nhiều endpoint Auth) |
| 11 | Nguồn XP bị vô hiệu hóa. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 12 | Phần thưởng XP đã được xử lý. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |
| 13 | Xác thực người dùng thành công. | N/A | Shared auth service (nhiều endpoint Auth) |
| 14 | XP được cấp thành công. | N/A | Shared XP service (nhiều endpoint Gameplay/LearningPath/XP) |










