using UnityEngine;

public class EndGameCheckingManager
{
    // Giả sử bàn cờ kích thước 15x15
    // 0: Ô trống, 1: Player X, 2: Player O
    private int[,] board;

    public void Init(int[,] slotsStatus)
    {
        board = slotsStatus;
    }
    
    // Định nghĩa 4 trục bằng Vector2Int (Mỗi dòng là một cặp hướng đối lập)
    private readonly Vector2Int[][] checkAxes = new Vector2Int[][]
    {
        new Vector2Int[] { Vector2Int.left, Vector2Int.right },          // Trục ngang
        new Vector2Int[] { Vector2Int.up, Vector2Int.down },             // Trục dọc
        new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(1, -1) }, // Trục chéo chính (\)
        new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(-1, -1) }  // Trục chéo phụ (/)
    };

    /// <summary>
    /// Kiểm tra xem nước đi vừa rồi có tạo thành chuỗi thắng hay không.
    /// </summary>
    public bool CheckWin(int lastX, int lastY, int playerSign)
    {
        // Duyệt qua từng trục trong 4 trục
        foreach (Vector2Int[] axis in checkAxes)
        {
            int consecutiveCount = 1; // Tính luôn quân cờ vừa mới đánh
            board[lastX,lastY] = playerSign;
            // Duyệt 2 hướng đối lập trên cùng một trục
            foreach (Vector2Int direction in axis)
            {
                consecutiveCount += CountInDirection(lastX, lastY, direction.x, direction.y, playerSign);
            }

            // Nếu đạt từ 5 quân trở lên -> Thắng!
            if (consecutiveCount >= 5)
            {
                // Nếu bạn chơi luật Caro Việt Nam (chặn 2 đầu không tính), 
                // bạn có thể viết thêm một hàm kiểm tra chặn 2 đầu ở đây.
                return true; 
            }
        }

        return false;
    }

    /// <summary>
    /// Đếm số lượng quân cùng màu liên tiếp theo một hướng cụ thể
    /// </summary>
    private int CountInDirection(int startX, int startY, int dirX, int dirY, int playerSign)
    {
        int count = 0;
        int nextX = startX + dirX;
        int nextY = startY + dirY;

        // Chạy vòng lặp tiến tới chừng nào còn nằm trong bàn cờ và cùng màu quân cờ
        while (IsWithinBoard(nextX, nextY) && board[nextX, nextY] == playerSign)
        {
            count++;
            nextX += dirX;
            nextY += dirY;
        }

        return count;
    }

    /// <summary>
    /// Kiểm tra tọa độ có hợp lệ (nằm trong mảng) hay không
    /// </summary>
    private bool IsWithinBoard(int x, int y)
    {
        return x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(1);
    }
}