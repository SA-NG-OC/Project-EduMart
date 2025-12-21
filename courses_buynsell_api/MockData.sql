-- =============================================
-- COMPLETE SAMPLE DATA FOR COURSEHUB DATABASE
-- Date: 16/12/2025
-- =============================================

BEGIN;

-- =============================================
-- 1. USERS (Cập nhật IsEmailVerified = true)
-- =============================================
INSERT INTO public."Users" 
("FullName", "Email", "PasswordHash", "Role", "CreatedAt", "PhoneNumber", "IsEmailVerified", "RefreshToken", "RefreshTokenExpiryTime", "AvatarUrl")
VALUES
-- Admin
('Nguyễn Văn Admin', 'admin@coursehub.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Admin', NOW() - INTERVAL '365 days', '0901234567', true, NULL, NULL,
 'https://cdn.tgdd.vn/Products/Images/44/335362/macbook-air-13-inch-m4-xanh-da-troi-600x600.jpg'),

('Sang', 'sang@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Admin', NOW() - INTERVAL '300 days', '0900000000', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/327098/hp-15-fc0085au-r5-a6vv8pa-170225-110652-878-600x600.jpg'),

-- Sellers
('Trần Thị Hoa', 'hoa.tran@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Seller', NOW() - INTERVAL '200 days', '0912345678', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/358086/macbook-pro-14-inch-m5-16gb-512gb-thumb-638962954605863722-600x600.jpg'),

('Lê Văn Minh', 'minh.le@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Seller', NOW() - INTERVAL '180 days', '0923456789', true, NULL, NULL,
 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-7-1.jpg'),

('Phạm Thị Lan', 'lan.pham@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Seller', NOW() - INTERVAL '150 days', '0934567890', true, NULL, NULL,
 'https://cdn.tgdd.vn/Products/Images/44/335362/macbook-air-13-inch-m4-xanh-da-troi-600x600.jpg'),

('Hoàng Văn Nam', 'nam.hoang@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Seller', NOW() - INTERVAL '120 days', '0945678901', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/327098/hp-15-fc0085au-r5-a6vv8pa-170225-110652-878-600x600.jpg'),

-- Buyers
('Đỗ Văn Hùng', 'hung.do@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '100 days', '0956789012', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/358086/macbook-pro-14-inch-m5-16gb-512gb-thumb-638962954605863722-600x600.jpg'),

('Vũ Thị Mai', 'mai.vu@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '90 days', '0967890123', true, NULL, NULL,
 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-7-1.jpg'),

('Bùi Văn Tuấn', 'tuan.bui@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '80 days', '0978901234', true, NULL, NULL,
 'https://cdn.tgdd.vn/Products/Images/44/335362/macbook-air-13-inch-m4-xanh-da-troi-600x600.jpg'),

('Đinh Thị Hương', 'huong.dinh@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '70 days', '0989012345', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/327098/hp-15-fc0085au-r5-a6vv8pa-170225-110652-878-600x600.jpg'),

('Ngô Văn Long', 'long.ngo@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '60 days', '0990123456', true, NULL, NULL,
 'https://cdnv2.tgdd.vn/mwg-static/tgdd/Products/Images/44/358086/macbook-pro-14-inch-m5-16gb-512gb-thumb-638962954605863722-600x600.jpg'),

('Trương Thị Thu', 'thu.truong@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '50 days', '0901234568', true, NULL, NULL,
 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-7-1.jpg'),

('Nguyễn Văn An', 'an.nguyen@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Buyer', NOW() - INTERVAL '50 days', '0901234568', true, NULL, NULL,
 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-7-1.jpg'),

('Trương Thị Hương', 'huong.tran@gmail.com', 'Y6IE4cbribo1c2b2aq4IJg==.VJbIsl4pg2TOL3ZL4Xf2Pneez/s756/dq6ej15kjDPc=', 'Seller', NOW() - INTERVAL '50 days', '0901234568', true, NULL, NULL,
 'https://cdn2.cellphones.com.vn/insecure/rs:fill:358:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/s/a/samsung-galaxy-z-flip-7-1.jpg');

-- =============================================
-- 2. CATEGORIES
-- =============================================
INSERT INTO public."Categories" ("Name", "CreatedAt")
VALUES
('Khác', NOW() - INTERVAL '365 days'),
('Lập Trình', NOW() - INTERVAL '365 days'),
('Thiết Kế', NOW() - INTERVAL '365 days'),
('Kinh Doanh', NOW() - INTERVAL '365 days'),
('Marketing', NOW() - INTERVAL '365 days'),
('Phát Triển Cá Nhân', NOW() - INTERVAL '365 days'),
('Ngôn Ngữ', NOW() - INTERVAL '365 days');

-- =============================================
-- 3. COURSES (8 khóa học)
-- =============================================
INSERT INTO public."Courses" 
("Title", "Description", "Price", "Level", "TeacherName", "ImageUrl", "DurationHours", 
 "TotalPurchased", "AverageRating", "CreatedAt", "UpdatedAt", "SellerId", "CategoryId", 
 "IsApproved", "IsRestricted", "CourseLecture")
VALUES
-- Course 1 - Trương Thị Hương (SellerId = 14)
('Full Stack Web Development 2025', 'Khóa học lập trình web toàn diện từ cơ bản đến nâng cao', 2500000, 'Intermediate', 'Trương Thị Hương',
 'https://images.unsplash.com/photo-1633356122544-f134324a6cee', 120, 145, 4.7,
 NOW() - INTERVAL '180 days', NOW() - INTERVAL '5 days', 14, 1, true, false,
 'Giới thiệu về HTML, CSS, JavaScript, React, Node.js và MongoDB'),

-- Course 2 - Trương Thị Hương (SellerId = 14)
('UI/UX Design Masterclass', 'Thiết kế giao diện và trải nghiệm người dùng chuyên nghiệp', 1800000, 'Beginner', 'Trương Thị Hương',
 'https://images.unsplash.com/photo-1526379095098-d400fd0bf935', 80, 98, 4.6,
 NOW() - INTERVAL '150 days', NOW() - INTERVAL '10 days', 14, 2, true, false,
 'Figma, Adobe XD, Design Thinking, User Research'),

-- Course 3 - Trần Thị Hoa (SellerId = 3)
('Digital Marketing Strategy', 'Chiến lược marketing online hiệu quả cho doanh nghiệp', 1500000, 'Intermediate', 'Trần Thị Hoa',
 'https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c', 60, 187, 4.8,
 NOW() - INTERVAL '200 days', NOW() - INTERVAL '3 days', 3, 4, true, false,
 'SEO, SEM, Social Media Marketing, Content Marketing, Analytics'),

-- Course 4 - Lê Văn Minh (SellerId = 4)
('Python for Data Science', 'Phân tích dữ liệu và Machine Learning với Python', 2200000, 'Advanced', 'Lê Văn Minh',
 'https://images.unsplash.com/photo-1626785774573-4b799315345d', 100, 76, 4.5,
 NOW() - INTERVAL '160 days', NOW() - INTERVAL '7 days', 4, 1, true, false,
 'NumPy, Pandas, Matplotlib, Scikit-learn, TensorFlow'),

-- Course 5 - Phạm Thị Lan (SellerId = 5)
('Business Management Fundamentals', 'Kiến thức cơ bản về quản trị kinh doanh', 1200000, 'Beginner', 'Phạm Thị Lan',
 'https://images.unsplash.com/photo-1611162617474-5b21e879e113', 50, 134, 4.4,
 NOW() - INTERVAL '140 days', NOW() - INTERVAL '12 days', 5, 3, true, false,
 'Strategic Planning, Finance, HR, Operations Management'),

-- Course 6 - Hoàng Văn Nam (SellerId = 6)
('Mobile App Development with Flutter', 'Xây dựng ứng dụng di động đa nền tảng', 2800000, 'Intermediate', 'Hoàng Văn Nam',
 'https://images.unsplash.com/photo-1627398242454-45a1465c2479', 90, 112, 4.9,
 NOW() - INTERVAL '120 days', NOW() - INTERVAL '8 days', 6, 1, true, false,
 'Flutter, Dart, Firebase, State Management, API Integration'),

-- Course 7 - Trương Thị Hương (SellerId = 14)
('English for Business Communication', 'Tiếng Anh giao tiếp trong môi trường doanh nghiệp', 1600000, 'Intermediate', 'Trương Thị Hương',
 'https://images.unsplash.com/photo-1546410531-bb4caa6b424d', 70, 156, 4.7,
 NOW() - INTERVAL '100 days', NOW() - INTERVAL '6 days', 14, 6, true, false,
 'Business Writing, Presentation Skills, Negotiation, Email Communication'),

-- Course 8 - Trần Thị Hoa (SellerId = 3)
('Financial Analysis and Investment', 'Phân tích tài chính và đầu tư chứng khoán', 1900000, 'Advanced', 'Trần Thị Hoa',
 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40', 85, 89, 4.6,
 NOW() - INTERVAL '90 days', NOW() - INTERVAL '4 days', 3, 3, true, false,
 'Financial Statements, Valuation, Portfolio Management, Risk Analysis');

-- =============================================
-- 4. COURSE CONTENTS
-- =============================================
INSERT INTO public."CourseContents" ("Title", "Description", "CourseId")
VALUES
-- Course 1
('Giới thiệu về Web Development', 'Tổng quan về lập trình web và các công nghệ cần thiết', 1),
('HTML & CSS Basics', 'Nền tảng về HTML5 và CSS3', 1),
('JavaScript Fundamentals', 'Lập trình JavaScript cơ bản và nâng cao', 1),
('React Framework', 'Xây dựng giao diện với React', 1),
('Backend với Node.js', 'Lập trình server-side với Node.js và Express', 1),

-- Course 2
('Introduction to UI/UX', 'Khái niệm về thiết kế giao diện và trải nghiệm người dùng', 2),
('Design Principles', 'Nguyên tắc thiết kế cơ bản', 2),
('Figma Mastery', 'Làm chủ công cụ Figma', 2),
('User Research', 'Nghiên cứu người dùng và phân tích hành vi', 2),

-- Course 3
('Digital Marketing Overview', 'Tổng quan về marketing online', 3),
('SEO Optimization', 'Tối ưu hóa công cụ tìm kiếm', 3),
('Social Media Marketing', 'Marketing trên mạng xã hội', 3),
('Content Strategy', 'Chiến lược nội dung hiệu quả', 3),

-- Course 4
('Python Basics', 'Cú pháp Python cơ bản', 4),
('Data Analysis with Pandas', 'Phân tích dữ liệu với Pandas', 4),
('Machine Learning Intro', 'Giới thiệu Machine Learning', 4),

-- Course 5
('Business Strategy', 'Chiến lược kinh doanh', 5),
('Financial Management', 'Quản trị tài chính doanh nghiệp', 5),
('Human Resources', 'Quản lý nhân sự', 5),

-- Course 6
('Flutter Setup', 'Cài đặt và cấu hình Flutter', 6),
('Widget Development', 'Phát triển widgets', 6),
('State Management', 'Quản lý state trong Flutter', 6),

-- Course 7
('Business English Basics', 'Tiếng Anh kinh doanh cơ bản', 7),
('Presentation Skills', 'Kỹ năng thuyết trình', 7),
('Writing Business Emails', 'Viết email chuyên nghiệp', 7),

-- Course 8
('Reading Financial Statements', 'Đọc báo cáo tài chính', 8),
('Investment Strategies', 'Chiến lược đầu tư', 8),
('Risk Management', 'Quản lý rủi ro đầu tư', 8);

-- =============================================
-- 5. COURSE SKILLS
-- =============================================
INSERT INTO public."CourseSkills" ("Name", "CourseId")
VALUES
('HTML5', 1), ('CSS3', 1), ('JavaScript', 1), ('React', 1), ('Node.js', 1),
('Figma', 2), ('Adobe XD', 2), ('UI Design', 2), ('UX Research', 2),
('SEO', 3), ('Google Ads', 3), ('Facebook Ads', 3), ('Content Marketing', 3),
('Python', 4), ('Pandas', 4), ('NumPy', 4), ('Machine Learning', 4),
('Strategic Planning', 5), ('Leadership', 5), ('Financial Analysis', 5),
('Flutter', 6), ('Dart', 6), ('Firebase', 6), ('Mobile Development', 6),
('Business English', 7), ('Presentation', 7), ('Communication', 7),
('Financial Analysis', 8), ('Investment', 8), ('Portfolio Management', 8);

-- =============================================
-- 6. TARGET LEARNERS
-- =============================================
INSERT INTO public."TargetLearners" ("Description", "CourseId")
VALUES
('Người mới bắt đầu muốn trở thành lập trình viên web', 1),
('Sinh viên CNTT muốn nâng cao kỹ năng', 1),
('Designer muốn chuyển sang UI/UX', 2),
('Người có nền tảng thiết kế đồ họa', 2),
('Chủ doanh nghiệp nhỏ và vừa', 3),
('Nhân viên marketing muốn nâng cao kỹ năng', 3),
('Người có kiến thức lập trình cơ bản', 4),
('Data Analyst muốn học Machine Learning', 4),
('Sinh viên kinh tế', 5),
('Người mới khởi nghiệp', 5),
('Lập trình viên muốn phát triển mobile app', 6),
('Người có kinh nghiệm lập trình', 6),
('Nhân viên văn phòng', 7),
('Người làm việc trong môi trường quốc tế', 7),
('Nhà đầu tư cá nhân', 8),
('Chuyên viên tài chính', 8);

-- =============================================
-- 7. ENROLLMENTS (3-4 người cho các khóa học của Trương Thị Hương)
-- =============================================
INSERT INTO public."Enrollments" ("EnrollAt", "BuyerId", "CourseId")
VALUES
-- Course 1 (Trương Thị Hương)
(NOW() - INTERVAL '45 days', 7, 1),
(NOW() - INTERVAL '40 days', 8, 1),
(NOW() - INTERVAL '35 days', 9, 1),
(NOW() - INTERVAL '30 days', 10, 1),

-- Course 2 (Trương Thị Hương)
(NOW() - INTERVAL '42 days', 7, 2),
(NOW() - INTERVAL '38 days', 11, 2),
(NOW() - INTERVAL '33 days', 12, 2),

-- Course 7 (Trương Thị Hương)
(NOW() - INTERVAL '25 days', 8, 7),
(NOW() - INTERVAL '20 days', 9, 7),
(NOW() - INTERVAL '15 days', 13, 7),
(NOW() - INTERVAL '10 days', 10, 7),

-- Các khóa học khác (thêm đa dạng)
(NOW() - INTERVAL '50 days', 7, 3),
(NOW() - INTERVAL '48 days', 8, 4),
(NOW() - INTERVAL '46 days', 9, 5),
(NOW() - INTERVAL '44 days', 10, 6),
(NOW() - INTERVAL '42 days', 11, 8),
(NOW() - INTERVAL '40 days', 12, 3),
(NOW() - INTERVAL '38 days', 13, 4);

-- =============================================
-- 8. REVIEWS
-- =============================================
INSERT INTO public."Reviews" ("Star", "CreatedAt", "Comment", "BuyerId", "CourseId")
VALUES
(5, NOW() - INTERVAL '10 days', 'Khóa học rất hay và bổ ích. Giảng viên nhiệt tình!', 7, 1),
(4, NOW() - INTERVAL '8 days', 'Nội dung tốt nhưng hơi nhanh ở một số phần', 8, 1),
(5, NOW() - INTERVAL '6 days', 'Xuất sắc! Tôi đã học được rất nhiều', 9, 1),
(5, NOW() - INTERVAL '5 days', 'Khóa học thiết kế tuyệt vời', 7, 2),
(4, NOW() - INTERVAL '12 days', 'Marketing content rất thực tế', 7, 3),
(5, NOW() - INTERVAL '11 days', 'Python course rất chi tiết', 8, 4),
(4, NOW() - INTERVAL '9 days', 'Kiến thức kinh doanh hữu ích', 9, 5),
(5, NOW() - INTERVAL '7 days', 'Flutter course tốt nhất tôi từng học', 10, 6),
(5, NOW() - INTERVAL '4 days', 'Tiếng Anh giao tiếp cực kỳ hữu ích', 8, 7),
(4, NOW() - INTERVAL '3 days', 'Phân tích tài chính rất chuyên sâu', 11, 8);

-- =============================================
-- 9. TRANSACTIONS (12 tháng, 7 ngày gần nhất mỗi ngày có 1 transaction)
-- =============================================
INSERT INTO public."Transactions" 
("TransactionCode", "PaymentMethod", "TotalAmount", "CreatedAt", "UpdatedAt", "BuyerId")
VALUES
-- Tháng 12/2025 - 7 ngày gần nhất (10-16/12)
('TXN202512160001', 'Credit Card', 4300000, '2025-12-16 10:30:00+07', '2025-12-16 10:30:00+07', 7),
('TXN202512150001', 'Bank Transfer', 3700000, '2025-12-15 14:20:00+07', '2025-12-15 14:20:00+07', 8),
('TXN202512140001', 'E-Wallet', 5100000, '2025-12-14 09:15:00+07', '2025-12-14 09:15:00+07', 9),
('TXN202512130001', 'Credit Card', 4600000, '2025-12-13 16:45:00+07', '2025-12-13 16:45:00+07', 10),
('TXN202512120001', 'Bank Transfer', 2800000, '2025-12-12 11:30:00+07', '2025-12-12 11:30:00+07', 11),
('TXN202512110001', 'E-Wallet', 3400000, '2025-12-11 13:20:00+07', '2025-12-11 13:20:00+07', 12),
('TXN202512100001', 'Credit Card', 4800000, '2025-12-10 15:10:00+07', '2025-12-10 15:10:00+07', 13),

-- Tháng 12/2025 - đầu tháng
('TXN202512050001', 'Bank Transfer', 3900000, '2025-12-05 10:00:00+07', '2025-12-05 10:00:00+07', 7),
('TXN202512020001', 'Credit Card', 4200000, '2025-12-02 14:30:00+07', '2025-12-02 14:30:00+07', 8),

-- Tháng 11/2025
('TXN202511250001', 'E-Wallet', 3500000, '2025-11-25 11:15:00+07', '2025-11-25 11:15:00+07', 9),
('TXN202511150001', 'Bank Transfer', 4100000, '2025-11-15 09:30:00+07', '2025-11-15 09:30:00+07', 10),

-- Tháng 10/2025
('TXN202510220001', 'Credit Card', 3800000, '2025-10-22 13:45:00+07', '2025-10-22 13:45:00+07', 11),
('TXN202510120001', 'E-Wallet', 4400000, '2025-10-12 10:20:00+07', '2025-10-12 10:20:00+07', 12),

-- Tháng 9/2025
('TXN202509180001', 'Bank Transfer', 3600000, '2025-09-18 15:30:00+07', '2025-09-18 15:30:00+07', 7),

-- Tháng 8/2025
('TXN202508250001', 'Credit Card', 4000000, '2025-08-25 11:00:00+07', '2025-08-25 11:00:00+07', 8),
('TXN202508100001', 'E-Wallet', 3300000, '2025-08-10 14:15:00+07', '2025-08-10 14:15:00+07', 9),

-- Tháng 7/2025
('TXN202507200001', 'Bank Transfer', 4500000, '2025-07-20 09:45:00+07', '2025-07-20 09:45:00+07', 10),

-- Tháng 6/2025
('TXN202506280001', 'Credit Card', 3700000, '2025-06-28 16:20:00+07', '2025-06-28 16:20:00+07', 11),
('TXN202506150001', 'E-Wallet', 4300000, '2025-06-15 10:30:00+07', '2025-06-15 10:30:00+07', 12),

-- Tháng 5/2025
('TXN202505220001', 'Bank Transfer', 3900000, '2025-05-22 13:00:00+07', '2025-05-22 13:00:00+07', 13),

-- Tháng 4/2025
('TXN202504180001', 'Credit Card', 4100000, '2025-04-18 11:45:00+07', '2025-04-18 11:45:00+07', 7),

-- Tháng 3/2025
('TXN202503250001', 'E-Wallet', 3500000, '2025-03-25 14:30:00+07', '2025-03-25 14:30:00+07', 8),
('TXN202503120001', 'Bank Transfer', 4200000, '2025-03-12 09:15:00+07', '2025-03-12 09:15:00+07', 9),

-- Tháng 2/2025
('TXN202502200001', 'Credit Card', 3800000, '2025-02-20 15:45:00+07', '2025-02-20 15:45:00+07', 10),

-- Tháng 1/2025
('TXN202501280001', 'E-Wallet', 4000000, '2025-01-28 10:30:00+07', '2025-01-28 10:30:00+07', 11),
('TXN202501150001', 'Bank Transfer', 3600000, '2025-01-15 13:20:00+07', '2025-01-15 13:20:00+07', 12);

-- =============================================
-- 10. TRANSACTION DETAILS
-- =============================================
INSERT INTO public."TransactionDetails" ("Price", "TransactionId", "CourseId")
VALUES
-- Transaction 1 (16/12)
(2500000, 1, 1), (1800000, 1, 2),
-- Transaction 2 (15/12)
(1900000, 2, 8), (1800000, 2, 2),
-- Transaction 3 (14/12)
(2800000, 3, 6), (2200000, 3, 4), (100000, 3, 5),
-- Transaction 4 (13/12)
(2500000, 4, 1), (1900000, 4, 8), (200000, 4, 5),
-- Transaction 5 (12/12)
(1600000, 5, 7), (1200000, 5, 5),
-- Transaction 6 (11/12)
(1500000, 6, 3), (1900000, 6, 8),
-- Transaction 7 (10/12)
(2500000, 7, 1), (2200000, 7, 4), (100000, 7, 5),

-- Transaction 8 (05/12)
(1800000, 8, 2), (2200000, 8, 4), (100000, 8, 5),
-- Transaction 9 (02/12)
(2500000, 9, 1), (1600000, 9, 7), (100000, 9, 5),

-- Tháng 11
(1500000, 10, 3), (2000000, 10, 4),
(2800000, 11, 6), (1300000, 11, 5),

-- Tháng 10
(1800000, 12, 2), (2000000, 12, 4),
(2500000, 13, 1), (1900000, 13, 8),

-- Tháng 9
(1600000, 14, 7), (2000000, 14, 4),

-- Tháng 8
(2500000, 15, 1), (1500000, 15, 3),
(1500000, 16, 3), (1800000, 16, 2),

-- Tháng 7
(2800000, 17, 6), (1600000, 17, 7), (100000, 17, 5),

-- Tháng 6
(1900000, 18, 8), (1800000, 18, 2),
(2500000, 19, 1), (1800000, 19, 2),

-- Tháng 5
(1600000, 20, 7), (2200000, 20, 4), (100000, 20, 5),

-- Tháng 4
(2500000, 21, 1), (1600000, 21, 7),

-- Tháng 3
(1500000, 22, 3), (2000000, 22, 4),
(2500000, 23, 1), (1600000, 23, 7), (100000, 23, 5),

-- Tháng 2
(1800000, 24, 2), (2000000, 24, 4),

-- Tháng 1
(2500000, 25, 1), (1500000, 25, 3),
(1600000, 26, 7), (2000000, 26, 4);

-- =============================================
-- 11. CARTS
-- =============================================
INSERT INTO public."Carts" ("UserId", "CreatedAt")
VALUES
(7, NOW() - INTERVAL '5 days'),
(8, NOW() - INTERVAL '3 days'),
(9, NOW() - INTERVAL '2 days'),
(10, NOW() - INTERVAL '1 day');

-- =============================================
-- 12. CART ITEMS
-- =============================================
INSERT INTO public."CartItems" ("CartId", "CourseId")
VALUES
(1, 3), (1, 5),
(2, 4), (2, 6),
(3, 8),
(4, 3), (4, 7);

-- =============================================
-- 13. FAVORITES
-- =============================================
INSERT INTO public."Favorites" ("UserId", "CourseId")
VALUES
(7, 1), (7, 3), (7, 6),
(8, 2), (8, 4), (8, 7),
(9, 1), (9, 5), (9, 8),
(10, 3), (10, 6),
(11, 2), (11, 4), (11, 7),
(12, 1), (12, 8),
(13, 5), (13, 6);

-- =============================================
-- 14. HISTORIES
-- =============================================
INSERT INTO public."Histories" ("UserId", "CourseId", "CreatedAt")
VALUES
(7, 1, NOW() - INTERVAL '2 hours'),
(7, 2, NOW() - INTERVAL '5 hours'),
(7, 3, NOW() - INTERVAL '1 day'),
(8, 4, NOW() - INTERVAL '3 hours'),
(8, 5, NOW() - INTERVAL '8 hours'),
(9, 1, NOW() - INTERVAL '4 hours'),
(9, 6, NOW() - INTERVAL '10 hours'),
(10, 7, NOW() - INTERVAL '6 hours'),
(10, 8, NOW() - INTERVAL '1 day'),
(11, 2, NOW() - INTERVAL '7 hours'),
(12, 3, NOW() - INTERVAL '9 hours'),
(13, 1, NOW() - INTERVAL '5 hours');

-- =============================================
-- 15. CONVERSATIONS
-- =============================================
INSERT INTO public."Conversations" 
("CourseId", "BuyerId", "SellerId", "CreatedAt", "LastMessageAt")
VALUES
(1, 7, 14, NOW() - INTERVAL '10 days', NOW() - INTERVAL '1 day'),
(2, 8, 14, NOW() - INTERVAL '8 days', NOW() - INTERVAL '2 hours'),
(3, 7, 3, NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 days'),
(4, 9, 4, NOW() - INTERVAL '12 days', NOW() - INTERVAL '3 days'),
(7, 10, 14, NOW() - INTERVAL '6 days', NOW() - INTERVAL '12 hours');

-- =============================================
-- 16. MESSAGES
-- =============================================
INSERT INTO public."Messages" 
("ConversationId", "SenderId", "Content", "CreatedAt", "IsRead")
VALUES
-- Conversation 1
(1, 7, 'Xin chào, tôi muốn hỏi về khóa học Full Stack', NOW() - INTERVAL '10 days', true),
(1, 14, 'Chào bạn! Khóa học bao gồm HTML, CSS, JavaScript, React và Node.js', NOW() - INTERVAL '10 days', true),
(1, 7, 'Cảm ơn! Thời lượng khóa học là bao lâu?', NOW() - INTERVAL '9 days', true),
(1, 14, 'Khóa học kéo dài 120 giờ, bạn có thể học theo tốc độ của mình', NOW() - INTERVAL '9 days', true),
(1, 7, 'Tôi đã đăng ký rồi, cảm ơn nhiều!', NOW() - INTERVAL '1 day', true),

-- Conversation 2
(2, 8, 'Khóa học UI/UX có phù hợp với người mới không?', NOW() - INTERVAL '8 days', true),
(2, 14, 'Hoàn toàn phù hợp! Khóa học bắt đầu từ cơ bản', NOW() - INTERVAL '8 days', true),
(2, 8, 'Tuyệt vời! Tôi sẽ đăng ký ngay', NOW() - INTERVAL '2 hours', false),

-- Conversation 3
(3, 7, 'Khóa học Digital Marketing có certificate không?', NOW() - INTERVAL '15 days', true),
(3, 3, 'Có bạn nhé! Sau khi hoàn thành bạn sẽ nhận được chứng chỉ', NOW() - INTERVAL '15 days', true),
(3, 7, 'Cảm ơn bạn!', NOW() - INTERVAL '5 days', true),

-- Conversation 4
(4, 9, 'Python course có thực hành không?', NOW() - INTERVAL '12 days', true),
(4, 4, 'Có nhiều bài tập thực hành và dự án thực tế', NOW() - INTERVAL '12 days', true),
(4, 9, 'Được rồi, thanks!', NOW() - INTERVAL '3 days', true),

-- Conversation 5
(5, 10, 'Tôi có thể học English course từ đâu?', NOW() - INTERVAL '6 days', true),
(5, 14, 'Bạn nên có kiến thức tiếng Anh cơ bản trước', NOW() - INTERVAL '6 days', true),
(5, 10, 'OK, tôi hiểu rồi', NOW() - INTERVAL '12 hours', false);

-- =============================================
-- 17. NOTIFICATIONS
-- =============================================
INSERT INTO public."Notifications" 
("Message", "CreatedAt", "IsRead", "SellerId")
VALUES
('Bạn có đơn hàng mới từ khóa học Full Stack Web Development', NOW() - INTERVAL '1 day', true, 14),
('Học viên mới đăng ký khóa học UI/UX Design', NOW() - INTERVAL '2 days', true, 14),
('Bạn nhận được review 5 sao cho khóa học Digital Marketing', NOW() - INTERVAL '3 days', false, 3),
('Có tin nhắn mới từ học viên', NOW() - INTERVAL '2 hours', false, 14),
('Đơn hàng mới cho khóa học Python for Data Science', NOW() - INTERVAL '5 days', true, 4),
('Học viên mới đăng ký khóa học Business Management', NOW() - INTERVAL '6 days', true, 5),
('Có tin nhắn mới từ học viên về khóa học Flutter', NOW() - INTERVAL '12 hours', false, 6),
('Review mới cho khóa học English for Business', NOW() - INTERVAL '4 days', true, 14);

COMMIT;