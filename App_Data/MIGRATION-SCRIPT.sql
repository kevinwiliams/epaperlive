USE [Observer_new]
GO

INSERT INTO [dbo].[subscriber]
           ([emailAddress]
           ,[firstName]
           ,[lastName]
           ,[passwordHash]
		   ,[token]
           ,[secretquestion]
           ,[secretans]
           ,[ipAddress]
           ,[isActive]
           ,[phoneNumber]
           ,[newsletter]
           ,[createdAt]
           ,[roleID])
           (SELECT	email, fname, lname, password, token,  secretQuestion, secretAnswer, ip,  
			CASE WHEN active = 'yes' THEN 1 ELSE 0 END AS 'active',  phone, 
			CASE WHEN newsletter = 'yes' THEN 1 ELSE 0 END AS 'newsletter', GETDATE(), 
			CASE WHEN email like '%jamaicaobserver.com' THEN '9003' ELSE '9002' END
			FROM  [dbo].[eusers]
			)
GO

INSERT INTO [dbo].[subscriber_address]
           ([SubscriberID]
            ,[addressType]
           ,[emailAddress]
           ,[addressLine1]
           ,[cityTown]
           ,[stateParish]
           ,[zipCode]
           ,[country]
           ,[createdAt])
           (SELECT(SELECT SubscriberID FROM [dbo].[subscriber] WHERE emailAddress = [email])
            ,'M'
			,[email]
			,CASE WHEN [address] IS NULL THEN 'N/A' ELSE SUBSTRING([address], 1, case when  CHARINDEX(',', [address] ) = 0 then 50 else CHARINDEX(',', [address]) -1 end) END AS Line1
			--,SUBSTRING([address], case when  CHARINDEX(',', [address] ) = 0 then LEN([address]) else CHARINDEX(',', [address])  end, 50) AS Line2
			--,SUBSTRING([address], (CHARINDEX(',', [address])+1), 50) AS Line2
			--,[address]
			,[city]
			,[state]
			,[zip]
			,CASE WHEN LEN([country]) > 3 THEN 'jm' ELSE 
			 CASE WHEN [country] IS NULL THEN 'us' ELSE [country] END END
			,GETDATE()
		FROM [dbo].[eusers])
GO

INSERT INTO [dbo].[subscriber_tranx]
           ([SubscriberID]
           ,[emailAddress]
		   ,[tranxAmount]
           ,[cardOwner]
           ,[cardType]
           ,[tranxDate]
           ,[rateID]
           ,[orderID]
           ,[ipAddress])
     --VALUES
           (SELECT 
		   (SELECT(SELECT top 1 SubscriberID FROM [dbo].[subscriber] WHERE emailAddress = [email]))
			  ,[email]
			  ,[cardAmount]
			  ,[cardOwnerName]
			  ,CASE WHEN [cardType] LIKE 'Comp%'THEN 'COMP' ELSE UPPER([cardType]) END
			  ,[transactionDate]
			  ,(SELECT TOP 1 [rateid] FROM [dbo].[printandsubrates] WHERE 
					CAST (( CAST([Rate] AS DECIMAL(10,2))) as nvarchar(50)) LIKE 
					CAST ((CAST([cardAmount] AS DECIMAL(10,2))) as nvarchar(50))  + '%')
			  ,[orderId]
			  ,[ip]
		  FROM [Observer_new].[dbo].[eusers]
		  WHERE [transactionDate] IS NOT NULL)
GO

INSERT INTO [dbo].[subscriber_epaper]
(
      [SubscriberID]
      ,[emailAddress]
      ,[rateID]
      ,[startDate]
      ,[endDate]
      ,[isActive]
      ,[subType]
      ,[createdAt]
	  )

SELECT(SELECT [SubscriberID] FROM [dbo].[subscriber] WHERE [emailAddress] = [email])
       ,e.[email]
	   ,CASE WHEN T.[rateID] IS NULL THEN 5 ELSE t.[rateID] END
      ,e.[subscriptionStart]
      ,e.[subscriptionEnd]
      ,1
	  ,t.[orderID]
      ,e.[transactionDate]
  FROM [dbo].[eusers] e
   JOIN [dbo].[subscriber_tranx] t ON t.EmailAddress = e.email
  WHERE [subscriptionEnd] > GETDATE();

GO

SELECT * FROM [dbo].[subscriber]
SELECT * FROM [dbo].[subscriber_address]
SELECT * FROM [dbo].[subscriber_epaper]
SELECT * FROM [dbo].[subscriber_tranx]



