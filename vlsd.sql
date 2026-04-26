--CREATE DATABASE VLXD;

USE VLXD;

CREATE TABLE Suppliers (
    SupplierID INT IDENTITY PRIMARY KEY,
    SupplierName NVARCHAR(150) NOT NULL,
    Phone VARCHAR(15) UNIQUE,
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    CategoryID INT IDENTITY PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(255)
);

CREATE TABLE Products (
    ProductID INT IDENTITY PRIMARY KEY,
    ProductName NVARCHAR(150) NOT NULL,
    CategoryID INT NOT NULL,
    SupplierID INT NOT NULL,
    Unit NVARCHAR(50) DEFAULT N'Bao',
    Price DECIMAL(12,2) CHECK (Price >= 0),
    StockQuantity INT DEFAULT 0 CHECK (StockQuantity >= 0),
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID),
    CONSTRAINT FK_Product_Supplier FOREIGN KEY (SupplierID)
        REFERENCES Suppliers(SupplierID)
);

CREATE TABLE Customers (
    CustomerID INT IDENTITY PRIMARY KEY,
    CustomerName NVARCHAR(150) NOT NULL,
    Phone VARCHAR(15),
    Address NVARCHAR(255),
    Email VARCHAR(100) UNIQUE
);

CREATE TABLE Employees (
    EmployeeID INT IDENTITY PRIMARY KEY,
    EmployeeName NVARCHAR(150) NOT NULL,
    Phone VARCHAR(15),
    Position NVARCHAR(100),
    HireDate DATE DEFAULT GETDATE()
);

CREATE TABLE Orders (
    OrderID INT IDENTITY PRIMARY KEY,
    CustomerID INT NOT NULL,
    EmployeeID INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(14,2) CHECK (TotalAmount >= 0),
    CONSTRAINT FK_Order_Customer FOREIGN KEY (CustomerID)
        REFERENCES Customers(CustomerID),
    CONSTRAINT FK_Order_Employee FOREIGN KEY (EmployeeID)
        REFERENCES Employees(EmployeeID)
);

CREATE TABLE OrderDetails (
    OrderID INT,
    ProductID INT,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(12,2) CHECK (UnitPrice >= 0),
    PRIMARY KEY (OrderID, ProductID),
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderID)
        REFERENCES Orders(OrderID),
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductID)
        REFERENCES Products(ProductID)
);

CREATE TABLE StockImports (
    ImportID INT IDENTITY PRIMARY KEY,
    SupplierID INT NOT NULL,
    ImportDate DATETIME DEFAULT GETDATE(),
    EmployeeID INT,
    CONSTRAINT FK_Import_Supplier FOREIGN KEY (SupplierID)
        REFERENCES Suppliers(SupplierID),
    CONSTRAINT FK_Import_Employee FOREIGN KEY (EmployeeID)
        REFERENCES Employees(EmployeeID)
);

CREATE TABLE StockImportDetails (
    ImportID INT,
    ProductID INT,
    Quantity INT CHECK (Quantity > 0),
    ImportPrice DECIMAL(12,2) CHECK (ImportPrice >= 0),
    PRIMARY KEY (ImportID, ProductID),
    CONSTRAINT FK_ImportDetail_Import FOREIGN KEY (ImportID)
        REFERENCES StockImports(ImportID),
    CONSTRAINT FK_ImportDetail_Product FOREIGN KEY (ProductID)
        REFERENCES Products(ProductID)
);

INSERT INTO Suppliers (SupplierName, Phone, Address)
VALUES
(N'Công ty Xi măng Hà Tiên', '0901111111', N'HCM'),
(N'Thép Hòa Phát', '0902222222', N'Hà Nội'),
(N'Gạch Đồng Tâm', '0903333333', N'Long An');

INSERT INTO Categories (CategoryName, Description)
VALUES
(N'Xi măng', N'Các loại xi măng xây dựng'),
(N'Thép', N'Thép xây dựng'),
(N'Gạch', N'Gạch xây tường');

INSERT INTO Products (ProductName, CategoryID, SupplierID, Unit, Price, StockQuantity)
VALUES
(N'Xi măng PCB40', 1, 1, N'Bao', 95000, 500),
(N'Thép phi 10', 2, 2, N'Cây', 120000, 300),
(N'Gạch đỏ', 3, 3, N'Viên', 1500, 10000),
(N'Xi măng Holcim', 1, 1, N'Bao', 100000, 200);

INSERT INTO Products(ProductName, CategoryID, SupplierID, Unit, Price, StockQuantity)
VALUES
(N'Xi măng Nghi Sơn', 2, 2, N'Bao', 100000, 0),
(N'Xi măng Long Sơn', 2, 2, N'Bao', 85000, 0),
(N'Xi măng SCG', 2, 2, N'Bao', 105000, 0);

INSERT INTO Customers (CustomerName, Phone, Address, Email)
VALUES
(N'Nguyễn Văn A', '0911111111', N'Nha Trang', 'a@gmail.com'),
(N'Trần Thị B', '0922222222', N'HCM', 'b@gmail.com'),
(N'Lê Văn C', '0933333333', N'Đà Nẵng', 'c@gmail.com');

INSERT INTO Employees (EmployeeName, Phone, Position)
VALUES
(N'Phạm Minh', '0944444444', N'Bán hàng'),
(N'Hoàng Anh', '0955555555', N'Quản lý');

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, TotalAmount)
VALUES
(1, 1, '2026-02-01', 950000),
(2, 1, '2026-02-02', 2400000),
(1, 2, '2026-02-05', 300000);

INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice)
VALUES
(1, 1, 10, 95000),
(2, 2, 20, 120000),
(3, 3, 200, 1500);

INSERT INTO StockImports (SupplierID, ImportDate, EmployeeID)
VALUES
(1, '2026-01-20', 2),
(2, '2026-01-22', 2);

INSERT INTO StockImportDetails (ImportID, ProductID, Quantity, ImportPrice)
VALUES
(1, 1, 200, 85000),
(2, 2, 100, 100000);

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, TotalAmount)
VALUES
(1, 1, '2026-02-10', 1900000),
(2, 2, '2026-02-11', 450000),
(3, 1, '2026-02-12', 3000000),
(1, 2, '2026-02-15', 750000),
(2, 1, '2026-02-18', 1200000);

INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice)
VALUES
-- Order 4
(4, 1, 20, 95000),   -- Xi măng PCB40
-- Order 5
(5, 3, 300, 1500),   -- Gạch đỏ
-- Order 6
(6, 2, 25, 120000),  -- Thép phi 10
-- Order 7
(7, 1, 5, 95000),    -- Xi măng PCB40
(7, 4, 5, 100000),   -- Xi măng Holcim
-- Order 8
(8, 2, 10, 120000);  -- Thép phi 10


INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, TotalAmount)
VALUES
(1, 1, '2026-03-10 8:30:00', 1900000);

CREATE TABLE AttendanceLogs (
    LogID INT IDENTITY PRIMARY KEY,
    EmployeeID INT NOT NULL,
    CheckTime DATETIME DEFAULT GETDATE(),
    Type VARCHAR(10),
    CONSTRAINT FK_Log_Employee FOREIGN KEY (EmployeeID)
        REFERENCES Employees(EmployeeID)
);

INSERT INTO AttendanceLogs(EmployeeID, CheckTime, Type)
VALUES
(1, '2026-03-10 8:00:00', 'IN'),
(1, '2026-03-10 17:00:00', 'OUT'),
(1, '2026-03-11 8:30:00', 'IN'),
(1, '2026-03-11 17:00:00', 'OUT');

INSERT INTO AttendanceLogs(EmployeeID, CheckTime, Type)
VALUES
(2, '2026-03-10 8:00:00', 'IN'),
(2, '2026-03-10 17:00:00', 'OUT')

INSERT INTO AttendanceLogs(EmployeeID, CheckTime, Type)
VALUES
(2, '2026-03-11 8:00:00', 'IN'),
(2, '2026-03-11 17:00:00', 'OUT');