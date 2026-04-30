# Phân tích chiến dịch khách hàng - ASP.NET Core MVC



## 1. Yêu cầu môi trường
- Visual Studio 2022
- .NET 8 SDK

## 2. Cách chạy
1. Mở file `MarketingDecisionSupport.csproj` bằng Visual Studio 2022.
2. Restore NuGet nếu Visual Studio yêu cầu.
3. Nhấn `Ctrl + F5` để chạy.
4. Vào màn hình **Nạp dữ liệu** để import:
   - `Data/Samples/sample_customers.csv`
   - `Data/Samples/sample_transactions.csv

## 4. Cấu trúc chính
- `Controllers/`: điều hướng màn hình
- `Services/`: xử lý CSV, tính toán nghiệp vụ, luật mua kèm
- `Models/`: model dữ liệu và view model
- `Views/`: giao diện Razor
- `Data/Samples/`: dữ liệu mẫu chạy thử
