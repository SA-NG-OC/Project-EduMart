namespace courses_buynsell_api.Interfaces;

public interface IBlockService
{
    // Hàm trả về true nếu chặn thành công, false nếu có lỗi hoặc đã chặn rồi
    Task<bool> BlockUserAsync(int sellerId, int userToBlockId);
    // Hàm mới: Gỡ chặn
    Task<bool> UnblockUserAsync(int sellerId, int userToUnblockId);

    // Hàm mới: Kiểm tra xem 2 người này có chặn nhau không (2 chiều)
    Task<bool> IsBlockedAsync(int userAId, int userBId);
}