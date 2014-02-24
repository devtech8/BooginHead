USE [Booginhead]
GO
/*
Add the Wholesaler table and the Wholesaler_Id column to the Customer table.
*/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Wholesaler](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TaxId] [nvarchar](50) NULL,
	[WebsiteURL] [nvarchar](250) NULL,
	[International] [bit] NULL,
	[HowDidYouHear] [nvarchar](250) NULL,
	[YearsInBusiness] [nvarchar](50) NULL,
	[StoreFront] [nvarchar](50) NULL,
	[TypeOfStore] [nvarchar](50) NULL,
	[NameOfWebStore] [nvarchar](50) NULL,
	[AmazonSellerName] [nvarchar](50) NULL,
	[AcceptedTerms] [bit] NULL CONSTRAINT [DF_Wholesaler_AcceptedTerms]  DEFAULT ((0)), 
	CONSTRAINT [PK_Wholesaler] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Customer] ADD [Wholesaler_Id] [int] null
GO
ALTER TABLE [dbo].[Customer]  WITH CHECK ADD  CONSTRAINT [FK_Customer_Wholesaler] FOREIGN KEY([Wholesaler_Id])
REFERENCES [dbo].[Wholesaler] ([Id])
GO
ALTER TABLE [dbo].[Customer] CHECK CONSTRAINT [FK_Customer_Wholesaler]
GO
