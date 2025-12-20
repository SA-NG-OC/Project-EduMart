using courses_buynsell_api.DTOs.Chat;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace courses_buynsell_api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;
    // ✅ THÊM DICTIONARY ĐỂ TRACK AI ĐANG Ở CONVERSATION NÀO
    private static readonly Dictionary<string, HashSet<int>> _conversationMembers = new();
    private static readonly object _lock = new();

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    private int GetUserId()
    {
        try
        {
            var userIdClaim = Context.User?.FindFirst("id")?.Value
                ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogInformation($"Got userId from JWT Claims: {userId}");
                return userId;
            }

            _logger.LogError("Cannot determine userId - Claims: {Claims}",
                string.Join(", ", Context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));

            throw new HubException("Không xác định được người dùng hiện tại.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting userId");
            throw new HubException($"Error getting userId: {ex.Message}");
        }
    }

    // ✅ THÊM VÀO TRACKING KHI JOIN
    public async Task JoinConversation(int conversationId)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} joining conversation {conversationId}");

            var hasAccess = await _chatService.HasAccessToConversationAsync(userId, conversationId);
            if (!hasAccess)
            {
                throw new HubException("You don't have access to this conversation");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

            // ✅ TRACK USER VÀO CONVERSATION
            lock (_lock)
            {
                var roomKey = $"conversation_{conversationId}";
                if (!_conversationMembers.ContainsKey(roomKey))
                {
                    _conversationMembers[roomKey] = new HashSet<int>();
                }
                _conversationMembers[roomKey].Add(userId);
            }

            await _chatService.MarkMessagesAsReadAsync(userId, conversationId);

            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("UserJoined", userId, Context.ConnectionId);

            _logger.LogInformation($"User {userId} successfully joined conversation {conversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error joining conversation {conversationId}");
            throw new HubException($"Error joining conversation: {ex.Message}");
        }
    }

    // ✅ XÓA KHỎI TRACKING KHI LEAVE
    public async Task LeaveConversation(int conversationId)
    {
        try
        {
            var userId = GetUserId();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

            // ✅ XÓA USER KHỎI TRACKING
            lock (_lock)
            {
                var roomKey = $"conversation_{conversationId}";
                if (_conversationMembers.ContainsKey(roomKey))
                {
                    _conversationMembers[roomKey].Remove(userId);
                    if (_conversationMembers[roomKey].Count == 0)
                    {
                        _conversationMembers.Remove(roomKey);
                    }
                }
            }

            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("UserLeft", userId, Context.ConnectionId);

            _logger.LogInformation($"User {userId} left conversation {conversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leaving conversation {conversationId}");
        }
    }

    // ✅ SỬA HÀM SendMessage
    public async Task SendMessage(SendMessageDto dto)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} sending message to conversation {dto.ConversationId}");

            var message = await _chatService.SendMessageAsync(userId, dto);

            // ✅ GỬI CHO TẤT CẢ NGƯỜI TRONG ROOM (bao gồm cả sender)
            await Clients.Group($"conversation_{dto.ConversationId}")
                .SendAsync("ReceiveMessage", message);

            // ✅ GỬI NOTIFICATION CHO NGƯỜI KHÔNG Ở TRONG ROOM
            await NotifyNewMessageToOfflineUsers(dto.ConversationId, userId, message);

            _logger.LogInformation($"Message sent successfully to conversation {dto.ConversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw new HubException($"Error sending message: {ex.Message}");
        }
    }

    public async Task UserTyping(int conversationId, bool isTyping)
    {
        try
        {
            var userId = GetUserId();

            var hasAccess = await _chatService.HasAccessToConversationAsync(userId, conversationId);
            if (!hasAccess)
            {
                throw new HubException("You don't have access to this conversation");
            }

            await Clients.OthersInGroup($"conversation_{conversationId}")
                .SendAsync("UserTypingStatus", userId, isTyping);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing status");
            throw new HubException($"Error sending typing status: {ex.Message}");
        }
    }

    public async Task MarkAsRead(int conversationId)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.MarkMessagesAsReadAsync(userId, conversationId);

            await Clients.OthersInGroup($"conversation_{conversationId}")
                .SendAsync("MessagesMarkedAsRead", userId, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read");
            throw new HubException($"Error marking messages as read: {ex.Message}");
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} connected to ChatHub, ConnectionId: {Context.ConnectionId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync");
            throw new HubException($"Error connecting: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetUserId();

            // ✅ XÓA USER KHỎI TẤT CẢ CONVERSATIONS KHI DISCONNECT
            lock (_lock)
            {
                foreach (var room in _conversationMembers.Keys.ToList())
                {
                    _conversationMembers[room].Remove(userId);
                    if (_conversationMembers[room].Count == 0)
                    {
                        _conversationMembers.Remove(room);
                    }
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"User {userId} disconnected from ChatHub");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in OnDisconnectedAsync (ignored)");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // ✅ SỬA HÀM NOTIFICATION - CHỈ GỬI CHO NGƯỜI KHÔNG Ở TRONG ROOM
    private async Task NotifyNewMessageToOfflineUsers(int conversationId, int senderId, MessageDto message)
    {
        try
        {
            var conversation = await _chatService.GetConversationDetailAsync(senderId, conversationId);
            if (conversation == null) return;

            var receiverId = conversation.BuyerId == senderId
                ? conversation.SellerId
                : conversation.BuyerId;

            // ✅ KIỂM TRA XEM NGƯỜI NHẬN CÓ ĐANG Ở TRONG ROOM KHÔNG
            bool isReceiverInRoom = false;
            lock (_lock)
            {
                var roomKey = $"conversation_{conversationId}";
                isReceiverInRoom = _conversationMembers.ContainsKey(roomKey)
                    && _conversationMembers[roomKey].Contains(receiverId);
            }

            // ✅ CHỈ GỬI NOTIFICATION NẾU NGƯỜI NHẬN KHÔNG Ở TRONG ROOM
            if (!isReceiverInRoom)
            {
                _logger.LogInformation($"Sending notification to user {receiverId} (not in room)");
                await Clients.Group($"user_{receiverId}")
                    .SendAsync("NewMessageNotification", new
                    {
                        Message = message,
                        Conversation = conversation
                    });
            }
            else
            {
                _logger.LogInformation($"User {receiverId} is in room, skipping notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error sending notification (ignored)");
        }
    }
}