USE PRN222_Lab_FPTCanteenLab06;
GO

-- Xoa du lieu cu (chay lai nhieu lan khong bi trung)
DELETE FROM TicketLines;
DELETE FROM Histories;
DELETE FROM OrderTickets;
DELETE FROM MenuItems;
GO

SET IDENTITY_INSERT MenuItems ON;
INSERT INTO MenuItems (Id, Name, Price, IsAvailable, Stall) VALUES
(1, N'Cơm gà', 25000, 1, N'Cơm'),
(2, N'Phở bò', 35000, 1, N'Phở'),
(3, N'Bánh mì', 15000, 1, N'Bánh mì'),
(4, N'Trà sữa', 20000, 1, N'Nước'),
(5, N'Mì xào', 28000, 1, N'Mì'),
(6, N'Coca', 10000, 1, N'Nước');
SET IDENTITY_INSERT MenuItems OFF;
GO

SET IDENTITY_INSERT OrderTickets ON;
INSERT INTO OrderTickets (Id, TicketCode, Stall, Note, Status, CreatedAt) VALUES
(1, 'P001', N'Cơm', N'Ít cay', 0, DATEADD(MINUTE, -15, GETDATE())),
(2, 'P002', N'Phở', N'', 0, GETDATE()),
(3, 'P003', N'Mì', N'Thêm trứng', 1, DATEADD(MINUTE, -5, GETDATE())),
(4, 'P004', N'Cơm', N'Mang về', 1, GETDATE()),
(5, 'P005', N'Nước', N'Ít đá', 2, GETDATE()),
(6, 'P006', N'Bánh mì', N'', 2, DATEADD(MINUTE, -12, GETDATE()));
SET IDENTITY_INSERT OrderTickets OFF;
GO

INSERT INTO TicketLines (OrderTicketId, MenuItemId, Quantity) VALUES
(1, 1, 2),
(1, 6, 1),
(2, 2, 1),
(3, 5, 1),
(4, 1, 1),
(4, 4, 1),
(5, 4, 2),
(6, 3, 2);
GO

-- Kiem tra
SELECT * FROM MenuItems;
SELECT * FROM OrderTickets;
SELECT * FROM TicketLines;
