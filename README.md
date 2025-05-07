# Hệ Thống Quản Lý Thư Viện

Đây là một ứng dụng quản lý thư viện đầy đủ tính năng được xây dựng với giao diện người dùng React (frontend) và .NET Core API (backend).

## Cấu Trúc Dự Án

Dự án được tổ chức thành hai phần chính:

### Frontend (client)
- Xây dựng bằng React, TypeScript và Vite
- Sử dụng Tailwind CSS cho styling
- Sử dụng React Query để quản lý trạng thái và tương tác API
- Giao diện UI sử dụng Radix UI components

### Backend (server)
- Xây dựng bằng .NET Core với kiến trúc Clean Architecture
- Cấu trúc theo các lớp:
  - **WebAPI**: API endpoints
  - **Application**: Logic nghiệp vụ
  - **Domain**: Các thực thể và model của hệ thống
  - **Infrastructure.Data**: Lớp cơ sở dữ liệu và repository
- Bao gồm các bộ test:
  - Unit Tests
  - Integration Tests
  - Functional Tests

## Tính Năng

- Quản lý sách
- Quản lý danh mục sách
- Quản lý người dùng
- Chức năng quản trị viên

## Cài Đặt

### Yêu Cầu

- Node.js (phiên bản mới nhất)
- .NET Core SDK 7.0 trở lên
- SQL Server hoặc cơ sở dữ liệu tương thích

### Cài Đặt Frontend

```bash
# Di chuyển vào thư mục client
cd client

# Cài đặt các gói phụ thuộc
npm install

# Chạy môi trường phát triển
npm run dev
```

### Cài Đặt Backend

```bash
# Di chuyển vào thư mục WebAPI
cd server/LibraryManagementSystem/WebAPI

# Khôi phục các gói NuGet
dotnet restore

# Chạy ứng dụng
dotnet run
```

### Cấu Hình Kết Nối Database

#### SQL Server

1. Chỉnh sửa chuỗi kết nối trong file `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=LibraryManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

2. Chạy migration để tạo database:

```bash
# Di chuyển vào thư mục Infrastructure.Data
cd server/LibraryManagementSystem/LibraryManagementSystem.Infrastructure.Data

# Tạo migration (nếu chưa có)
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

#### Sử Dụng Database Khác

Nếu bạn muốn sử dụng database khác (như MySQL, PostgreSQL):

1. Cài đặt provider phù hợp qua NuGet
2. Thay đổi chuỗi kết nối trong `appsettings.json`
3. Cập nhật `Startup.cs` để sử dụng provider tương ứng trong phương thức `ConfigureServices`:

```csharp
// Ví dụ với PostgreSQL
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
```

## Đóng Góp

Mọi đóng góp đều được chào đón. Hãy fork repository này, tạo một nhánh mới và gửi pull request.

## Giấy Phép

Dự án này được cấp phép theo [MIT License](LICENSE). 