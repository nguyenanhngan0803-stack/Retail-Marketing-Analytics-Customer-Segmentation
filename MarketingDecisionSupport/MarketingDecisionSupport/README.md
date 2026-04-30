# Phân tích chiến dịch khách hàng - ASP.NET Core MVC

Đây là bộ source C# được dựng từ luồng nghiệp vụ chính trong notebook Colab của bạn:
- nạp dữ liệu khách hàng và giao dịch
- tiền xử lý theo mốc năm 2014
- tạo chỉ số `Age`, `Total_Spent`, `Customer_Tenure_Days`
- phân cụm khách hàng bằng K-Means tự cài đặt đơn giản
- chấm điểm xác suất phản hồi theo cấu trúc ensemble RF / Linear SVM / LDA
- dự báo chi tiêu kỳ tới theo công thức nghiệp vụ
- xác định `Digital_Eligible`
- phân nhóm hành động A / B / C / D theo phân vị 90% / 70% / 40%
- tính `Expected_Gross_Profit`, `Expected_Profit_After_Campaign`, `Campaign_ROI`
- phân tích luật mua kèm từ file giao dịch

## 1. Yêu cầu môi trường
- Visual Studio 2022
- .NET 8 SDK

## 2. Cách chạy
1. Mở file `MarketingDecisionSupport.csproj` bằng Visual Studio 2022.
2. Restore NuGet nếu Visual Studio yêu cầu.
3. Nhấn `Ctrl + F5` để chạy.
4. Vào màn hình **Nạp dữ liệu** để import:
   - `Data/Samples/sample_customers.csv`
   - `Data/Samples/sample_transactions.csv`

## 3. Ghi chú quan trọng
- Bộ source này tập trung vào **luồng nghiệp vụ chính** của notebook để đưa lên sản phẩm C#.
- Các thuật toán nâng cao trong notebook như Fuzzy C-Means, SMOTE hay threshold tuning đầy đủ không được port 1:1 sang C#, nhưng cấu trúc ra quyết định và các màn hình quản trị đã được giữ theo tinh thần gốc của đồ án.
- Nếu bạn muốn phiên bản tiếp theo có SQL Server, đăng nhập, phân quyền, lưu lịch sử chiến dịch và xuất PDF/Excel, có thể mở rộng trực tiếp từ bộ source này.

## 4. Cấu trúc chính
- `Controllers/`: điều hướng màn hình
- `Services/`: xử lý CSV, tính toán nghiệp vụ, luật mua kèm
- `Models/`: model dữ liệu và view model
- `Views/`: giao diện Razor
- `Data/Samples/`: dữ liệu mẫu chạy thử

## 5. Màn hình đã có
- Tổng quan điều hành
- Nạp dữ liệu CSV
- Danh sách khách hàng
- Chi tiết từng khách hàng
- Tóm tắt chiến dịch
- Luật mua kèm
