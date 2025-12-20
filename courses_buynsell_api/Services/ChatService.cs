using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs.Chat;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _context;

    public ChatService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationDto> GetOrCreateConversationAsync(int buyerId, CreateConversationDto dto)
    {
        // Kiểm tra course tồn tại
        var course = await _context.Courses
            .Include(c => c.Seller)
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

        if (course == null)
            throw new Exception("Course not found");

        // Kiểm tra seller có đúng là owner của course không
        if (course.SellerId != dto.SellerId)
            throw new Exception("Invalid seller for this course");

        // Tìm conversation đã tồn tại
        var existingConversation = await _context.Conversations
            .Include(c => c.Course)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .FirstOrDefaultAsync(c =>
                c.CourseId == dto.CourseId &&
                c.BuyerId == buyerId &&
                c.SellerId == dto.SellerId);

        if (existingConversation != null)
        {
            return MapToConversationDto(existingConversation, buyerId);
        }

        // Tạo conversation mới
        var conversation = new Conversation
        {
            CourseId = dto.CourseId,
            BuyerId = buyerId,
            SellerId = dto.SellerId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Load lại với includes
        conversation = await _context.Conversations
            .Include(c => c.Course)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .FirstAsync(c => c.Id == conversation.Id);

        return MapToConversationDto(conversation, buyerId);
    }

    public async Task<PagedResult<ConversationDto>> GetUserConversationsAsync(int userId, int page, int pageSize)
    {
        // 1. Chỉ lọc hội thoại, KHÔNG dùng Include ở đây nữa (vì ta sẽ Select cụ thể bên dưới)
        var query = _context.Conversations
        .Where(c => (c.BuyerId == userId || c.SellerId == userId) && c.IsVisible);


        var total = await query.CountAsync();

        // 2. Dùng Select để map trực tiếp ra DTO.
        // EF Core sẽ dịch đoạn này thành câu SQL tối ưu: lấy thông tin + COUNT unread + TOP 1 message
        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConversationDto
            {
                Id = c.Id,
                CourseId = c.CourseId,
                CourseTitle = c.Course.Title, // Map các trường course

                // Map thông tin người chat cùng (Logic tương tự MapToConversationDto của bạn)
                BuyerId = c.BuyerId,
                BuyerName = c.Buyer.FullName, // Hoặc c.Buyer.UserName
                BuyerAvatar = c.Buyer.AvatarUrl,

                SellerId = c.SellerId,
                SellerName = c.Seller.FullName,
                SellerAvatar = c.Seller.AvatarUrl,

                LastMessageAt = c.LastMessageAt,

                // ✅ QUAN TRỌNG NHẤT: Đếm số tin nhắn chưa đọc ngay trong SQL
                UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId),

                // ✅ Lấy tin nhắn cuối cùng (Map sang MessageDto)
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt,
                        SenderId = m.SenderId,
                        IsRead = m.IsRead
                    })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return new PagedResult<ConversationDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = conversations
        };
    }

    public async Task<PagedResult<ConversationDto>> GetCourseConversationsAsync(
    int sellerId, int courseId, int page, int pageSize)
    {
        var courseExists = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.SellerId == sellerId);

        if (!courseExists)
            throw new Exception("Course not found or you don't have permission");

        // 1️⃣ Chỉ load buyer/seller + course (không load messages)
        var query = _context.Conversations
            .Include(c => c.Course)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Where(c => c.CourseId == courseId && c.SellerId == sellerId && c.IsVisible);

        var total = await query.CountAsync();

        var conversations = await query
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var conversationIds = conversations.Select(c => c.Id).ToList();

        // 2️⃣ Lấy last message cho toàn bộ conversation (1 query)
        var lastMessages = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new
            {
                ConversationId = g.Key,
                LastMessage = g
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.ConversationId, x => x.LastMessage);

        // 3️⃣ Lấy unread count (1 query)
        var unreadCounts = await _context.Messages
            .Where(m => conversationIds.Contains(m.ConversationId)
                        && !m.IsRead
                        && m.SenderId != sellerId)
            .GroupBy(m => m.ConversationId)
            .Select(g => new
            {
                ConversationId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.ConversationId, x => x.Count);

        // 4️⃣ Map DTO – bạn vẫn dùng map function hiện tại
        var items = conversations.Select(c =>
        {
            var dto = MapToConversationDto(c, sellerId);

            dto.LastMessage = lastMessages.ContainsKey(c.Id)
                ? new MessageDto
                {
                    Id = lastMessages[c.Id].Id,
                    Content = lastMessages[c.Id].Content,
                    CreatedAt = lastMessages[c.Id].CreatedAt,
                    SenderId = lastMessages[c.Id].SenderId,
                    IsRead = lastMessages[c.Id].IsRead
                }
                : null;

            dto.UnreadCount = unreadCounts.ContainsKey(c.Id)
                ? unreadCounts[c.Id]
                : 0;

            return dto;
        }).ToList();

        return new PagedResult<ConversationDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Items = items
        };
    }



    public async Task<MessageDto> SendMessageAsync(int senderId, SendMessageDto dto)
    {
        // Kiểm tra quyền truy cập conversation
        if (!await HasAccessToConversationAsync(senderId, dto.ConversationId))
            throw new Exception("You don't have access to this conversation");

        var message = new Message
        {
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);

        // Cập nhật LastMessageAt của conversation
        var conversation = await _context.Conversations
            .FirstAsync(c => c.Id == dto.ConversationId);

        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.IsVisible = true;

        await _context.SaveChangesAsync();

        // Load lại message với sender info
        message = await _context.Messages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id);

        return MapToMessageDto(message, senderId);
    }

    public async Task<PagedResult<MessageDto>> GetConversationMessagesAsync(int userId, GetMessagesDto dto)
    {
        // Kiểm tra quyền truy cập
        if (!await HasAccessToConversationAsync(userId, dto.ConversationId))
            throw new Exception("You don't have access to this conversation");

        // Tổng số message trong conversation
        var totalCount = await _context.Messages
            .Where(m => m.ConversationId == dto.ConversationId)
            .LongCountAsync();

        // Lấy dữ liệu phân trang
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == dto.ConversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((dto.Page - 1) * dto.PageSize)
            .Take(dto.PageSize)
            .ToListAsync();

        // Đảo lại để hiển thị tin cũ trước
        messages.Reverse();

        // Map DTO
        var items = messages
            .Select(m => MapToMessageDto(m, userId))
            .ToList();

        // Trả về PagedResult
        return new PagedResult<MessageDto>
        {
            Page = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = totalCount,
            Items = items
        };
    }


    public async Task MarkMessagesAsReadAsync(int userId, int conversationId)
    {
        // Kiểm tra quyền truy cập
        if (!await HasAccessToConversationAsync(userId, conversationId))
            throw new Exception("You don't have access to this conversation");

        // Đánh dấu tất cả tin nhắn chưa đọc mà user không phải là người gửi
        var unreadMessages = await _context.Messages
            .Where(m => m.ConversationId == conversationId &&
                       m.SenderId != userId &&
                       !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        if (unreadMessages.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasAccessToConversationAsync(int userId, int conversationId)
    {
        return await _context.Conversations
            .AnyAsync(c => c.Id == conversationId &&
                          (c.BuyerId == userId || c.SellerId == userId));
    }

    public async Task<ConversationDto?> GetConversationDetailAsync(int userId, int conversationId)
    {
        var conversation = await _context.Conversations
            .Include(c => c.Course)
            .Include(c => c.Buyer)
            .Include(c => c.Seller)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .FirstOrDefaultAsync(c => c.Id == conversationId &&
                                     (c.BuyerId == userId || c.SellerId == userId));

        return conversation != null ? MapToConversationDto(conversation, userId) : null;
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Messages
            .Where(m => m.Conversation.BuyerId == userId || m.Conversation.SellerId == userId)
            .Where(m => m.SenderId != userId && !m.IsRead)
            .CountAsync();
    }

    // Helper methods
    private ConversationDto MapToConversationDto(Conversation conversation, int currentUserId)
    {
        var lastMessage = conversation.Messages?.FirstOrDefault();
        var unreadCount = conversation.Messages?
            .Count(m => m.SenderId != currentUserId && !m.IsRead) ?? 0;

        return new ConversationDto
        {
            Id = conversation.Id,
            CourseId = conversation.CourseId,
            CourseTitle = conversation.Course?.Title ?? "",
            CourseImageUrl = conversation.Course?.ImageUrl ?? "",
            BuyerId = conversation.BuyerId,
            BuyerName = conversation.Buyer?.FullName ?? "",
            BuyerAvatar = conversation.Buyer?.AvatarUrl ?? "",
            SellerId = conversation.SellerId,
            SellerName = conversation.Seller?.FullName ?? "",
            SellerAvatar = conversation.Seller?.AvatarUrl ?? "",
            CreatedAt = conversation.CreatedAt,
            LastMessageAt = conversation.LastMessageAt,
            LastMessage = lastMessage != null ? MapToMessageDto(lastMessage, currentUserId) : null,
            UnreadCount = unreadCount
        };
    }

    private MessageDto MapToMessageDto(Message message, int currentUserId)
    {
        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = message.Sender?.FullName ?? "",
            SenderAvatar = message.Sender?.AvatarUrl ?? "",
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            IsRead = message.IsRead,
            IsSentByCurrentUser = message.SenderId == currentUserId
        };
    }

    public async Task<int> CountUnreadConversationsAsync(int userId)
    {
        return await _context.Messages
            .Include(m => m.Conversation)
            .Where(m =>
                m.SenderId != userId &&   // người gửi là đối phương
                !m.IsRead && m.Conversation!.SellerId == userId                 // chưa đọc
            )
            .Select(m => m.ConversationId)
            .Distinct()
            .CountAsync();
    }

    public async Task<List<UnreadConversationCountDto>> GetUnreadConversationsByCourseAsync(int sellerId)
    {
        var result = await _context.Conversations
            .Where(c => c.SellerId == sellerId)
            .Select(c => new
            {
                c.CourseId,
                Unread = c.Messages!
                    .Any(m => !m.IsRead && m.SenderId != sellerId)
            })
            .Where(x => x.Unread)
            .GroupBy(x => x.CourseId)
            .Select(g => new UnreadConversationCountDto
            {
                CourseId = g.Key,
                UnreadCount = g.Count()
            })
            .ToListAsync();

        return result;
    }

    // Thêm method này vào class ChatService
    public async Task<SendMessageWithConversationResponseDto> SendMessageWithNewConversationAsync(
        int buyerId,
        SendMessageWithNewConversationDto dto)
    {
        // 1. Validate Course tồn tại và thuộc về Seller
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId && c.SellerId == dto.SellerId);

        if (course == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy khóa học với ID {dto.CourseId} thuộc seller {dto.SellerId}");
        }

        // 2. Validate Buyer tồn tại
        var buyer = await _context.Users.FindAsync(buyerId);
        if (buyer == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy người dùng với ID {buyerId}");
        }

        // 3. Validate Seller tồn tại
        var seller = await _context.Users.FindAsync(dto.SellerId);
        if (seller == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy seller với ID {dto.SellerId}");
        }

        // 4. Kiểm tra xem đã có conversation chưa
        var existingConversation = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                c.CourseId == dto.CourseId &&
                c.BuyerId == buyerId &&
                c.SellerId == dto.SellerId);

        Conversation conversation;
        bool isNewConversation = false;

        if (existingConversation != null)
        {
            // Sử dụng conversation có sẵn
            conversation = existingConversation;
            existingConversation.IsVisible = true;
        }
        else
        {
            // Tạo conversation mới
            conversation = new Conversation
            {
                CourseId = dto.CourseId,
                BuyerId = buyerId,
                SellerId = dto.SellerId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            isNewConversation = true;
        }

        // 5. Tạo message mới
        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = buyerId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);

        // 6. Cập nhật LastMessageAt của conversation
        conversation.LastMessageAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // 7. Load lại để lấy thông tin đầy đủ
        var messageWithDetails = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Conversation)
                .ThenInclude(c => c.Course)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        if (messageWithDetails == null)
        {
            throw new Exception("Lỗi khi tải thông tin tin nhắn");
        }

        // 8. Map sang DTO
        var messageDto = new MessageDto
        {
            Id = messageWithDetails.Id,
            ConversationId = messageWithDetails.ConversationId,
            SenderId = messageWithDetails.SenderId,
            SenderName = messageWithDetails.Sender?.FullName ?? "Unknown",
            Content = messageWithDetails.Content,
            CreatedAt = messageWithDetails.CreatedAt,
            IsRead = messageWithDetails.IsRead
        };

        return new SendMessageWithConversationResponseDto
        {
            ConversationId = conversation.Id,
            Message = messageDto
        };
    }

    public async Task<List<ChatUserSearchResultDto>> SearchUsersAsync(int currentUserId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<ChatUserSearchResultDto>();

        // 1. Chuyển query về chữ thường và xóa khoảng trắng thừa
        query = query.Trim().ToLower();

        var results = await _context.Conversations
            .AsNoTracking()
            .Include(c => c.Buyer)
            .Include(c => c.Course)
            .Where(c => c.SellerId == currentUserId &&
                        // 2. Chuyển tên Buyer về chữ thường, sau đó dùng Contains
                        // Contains trong EF Core sẽ dịch ra SQL là LIKE '%query%'
                        c.Buyer.FullName.ToLower().Contains(query))
            .OrderByDescending(c => c.LastMessageAt)
            .Take(20)
            .Select(c => new ChatUserSearchResultDto
            {
                ConversationId = c.Id,
                BuyerId = c.BuyerId,
                BuyerName = c.Buyer.FullName, // Lưu ý: Trả về tên gốc (có hoa thường) để hiển thị cho đẹp
                BuyerAvatar = c.Buyer.AvatarUrl,
                CourseTitle = c.Course.Title,
                LastMessageAt = c.LastMessageAt
            })
            .ToListAsync();

        return results;
    }

    public async Task<bool> HideConversationAsync(int userId, int conversationId)
    {
        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c =>
                c.Id == conversationId &&
                c.SellerId == userId
            );

        if (conversation == null)
            return false;

        conversation.IsVisible = false;
        await _context.SaveChangesAsync();

        return true;
    }


}