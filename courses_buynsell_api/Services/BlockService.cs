using courses_buynsell_api.Entities;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Data;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services.Implements;

public class BlockService : IBlockService
{
    private readonly AppDbContext _context;

    public BlockService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> BlockUserAsync(int sellerId, int userToBlockId)
    {
        // 1. Không cho phép tự chặn chính mình
        if (sellerId == userToBlockId) return false;

        // 2. Tìm xem Seller này đã có bản ghi Block nào chưa
        var blockEntry = await _context.Blocks
            .FirstOrDefaultAsync(b => b.SellerId == sellerId);

        if (blockEntry == null)
        {
            // Trường hợp 1: Chưa từng chặn ai -> Tạo dòng mới
            var newBlock = new Block
            {
                SellerId = sellerId,
                BlockedUserIds = new int[] { userToBlockId } // Khởi tạo mảng với 1 phần tử
            };

            _context.Blocks.Add(newBlock);
        }
        else
        {
            // Trường hợp 2: Đã có bảng ghi -> Cập nhật mảng

            // Kiểm tra xem user kia đã bị chặn chưa để tránh trùng lặp
            if (blockEntry.BlockedUserIds.Contains(userToBlockId))
            {
                return true; // Đã chặn rồi, coi như thành công
            }

            // Chuyển mảng hiện tại sang List để dễ thao tác
            var currentList = blockEntry.BlockedUserIds.ToList();
            currentList.Add(userToBlockId);

            // Gán lại vào Entity (EF Core sẽ nhận diện sự thay đổi này)
            blockEntry.BlockedUserIds = currentList.ToArray();

            _context.Blocks.Update(blockEntry);
        }

        var conversations = await _context.Conversations
        .Where(c => c.SellerId == sellerId && c.BuyerId == userToBlockId)
        .ToListAsync();

        foreach (var c in conversations)
            c.IsBlock = true;

        // 3. Lưu xuống Database
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UnblockUserAsync(int sellerId, int userToUnblockId)
    {
        var blockEntry = await _context.Blocks
            .FirstOrDefaultAsync(b => b.SellerId == sellerId);

        // Chưa từng chặn ai hoặc danh sách rỗng
        if (blockEntry == null || !blockEntry.BlockedUserIds.Contains(userToUnblockId))
        {
            return false; // Coi như thất bại hoặc không cần làm gì
        }

        // Chuyển sang List để xóa
        var currentList = blockEntry.BlockedUserIds.ToList();
        currentList.Remove(userToUnblockId);

        // Cập nhật lại mảng
        blockEntry.BlockedUserIds = currentList.ToArray();

        // Nếu mảng rỗng thì có thể cân nhắc xóa luôn record Block (tuỳ logic, ở đây mình chỉ update)
        _context.Blocks.Update(blockEntry);

        var conversations = await _context.Conversations
        .Where(c => c.SellerId == sellerId && c.BuyerId == userToUnblockId)
        .ToListAsync();

        foreach (var c in conversations)
            c.IsBlock = false;

        await _context.SaveChangesAsync();

        return true;
    }

    // --- LOGIC MỚI: CHECK BLOCKED (Dùng cho Chat) ---
    public async Task<bool> IsBlockedAsync(int userAId, int userBId)
    {
        // Kiểm tra xem A có chặn B không? HOẶC B có chặn A không?
        // Lưu ý: Tuỳ vào DB Provider (Postgres/SQLServer) mà cách query mảng int[] sẽ khác nhau.
        // Đây là cách an toàn nhất: lấy record của cả 2 ra check.

        var blocks = await _context.Blocks
            .Where(b => b.SellerId == userAId || b.SellerId == userBId)
            .ToListAsync();

        // 1. Check A chặn B
        var aBlockB = blocks.FirstOrDefault(b => b.SellerId == userAId);
        if (aBlockB != null && aBlockB.BlockedUserIds.Contains(userBId))
            return true;

        // 2. Check B chặn A
        var bBlockA = blocks.FirstOrDefault(b => b.SellerId == userBId);
        if (bBlockA != null && bBlockA.BlockedUserIds.Contains(userAId))
            return true;

        return false;
    }
}