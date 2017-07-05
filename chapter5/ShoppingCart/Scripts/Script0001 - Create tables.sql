CREATE TABLE [shopcart].[ShoppingCart](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
  [UserId] [bigint] NOT NULL,
  CONSTRAINT ShoppingCartUnique UNIQUE([ID], [UserID])
)
GO

CREATE INDEX ShoppingCart_UserId 
  ON [shopcart].[ShoppingCart] (UserId)
GO

CREATE TABLE [shopcart].[ShoppingCartItems](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
  [ShoppingCartId] [int] NOT NULL,
  [ProductCatalogId] [bigint] NOT NULL,
  [ProductName] [nvarchar](100) NOT NULL,
  [ProductDescription] [nvarchar](500) NULL,
  [Amount] [decimal] NOT NULL,
  [Currency] [nvarchar](5) NOT NULL
)
GO

ALTER TABLE [shopcart].[ShoppingCartItems]  WITH CHECK ADD CONSTRAINT [FK_ShoppingCart] FOREIGN KEY([ShoppingCartId])
  REFERENCES [shopcart].[ShoppingCart] ([Id])
GO

ALTER TABLE [shopcart].[ShoppingCartItems] CHECK CONSTRAINT [FK_ShoppingCart]
GO

CREATE INDEX ShoppingCartItems_ShoppingCartId 
  ON [shopcart].[ShoppingCartItems] (ShoppingCartId)
GO

CREATE TABLE [shopcart].[EventStore](
  [ID] int IDENTITY(1,1) PRIMARY KEY,
  [Name] [nvarchar](100)  NOT NULL,
  [OccurredAt] [datetimeoffset] NOT NULL,
  [Content][nvarchar](max) NOT NULL
)
GO
