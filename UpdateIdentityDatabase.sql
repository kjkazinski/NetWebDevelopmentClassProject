CREATE TABLE [dbo].[CartItems] (
    [CartItemId]     INT IDENTITY (1, 1) NOT NULL,
    [ProductId]  INT NOT NULL,
    [CustomerId] INT NOT NULL,
    [Quantity]   INT NOT NULL,
    CONSTRAINT [PK_Cart] PRIMARY KEY CLUSTERED ([CartItemId] ASC),
    CONSTRAINT [FK_Cart_Products] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([ProductID]),
    CONSTRAINT [FK_Cart_Customers] FOREIGN KEY ([CustomerId]) REFERENCES [dbo].[Customers] ([CustomerId])
);
