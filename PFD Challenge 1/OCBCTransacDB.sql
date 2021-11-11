/********************************************************/
/*   OCBCTransacDB.sql	 							*/
/*   =============								*/
/*   Script File for setting up the database.	*/
/*   Last Modified Date: 1/11/2021			*/
/*												*/
/*   Upon successful setup, there should be:		*/
/*   4 Tables: User, BankAccount, FutureTransfer, Transaction */
/*   6 Category records, 4 Publisher records and 	*/
/*   8 Title records.								*/
/********************************************************/

CREATE database OCBCTransacDB
GO

USE OCBCTransacDB
GO



/****** Object:  Table dbo.Transaction  ******/
if exists (select * from sysobjects where name='Transactions' and type='U')
	drop table Transactions
GO
/****** Object:  Table dbo.FutureTransfer  ******/
if exists (select * from sysobjects where name='FutureTransfer' and type='U')
	drop table FutureTransfer
GO
/****** Object:  Table dbo.BankAccount  ******/
if exists (select * from sysobjects where name='BankAccount' and type='U')
	drop table BankAccount
GO
/****** Object:  Table dbo.User  ******/
if exists (select * from sysobjects where name='BankUser' and type='U')
	drop table BankUser
GO
/****** Object:  Table User ******/
CREATE TABLE BankUser (
	NRIC CHAR(9) NOT NULL,
	Email varchar (50) NOT NULL,
	Phone char (10) NOT NULL,
	Name varchar (50) NOT NULL,
	TransLimit smallmoney NOT NULL,
	Password varchar (255) NOT NULL
	CONSTRAINT PK_User PRIMARY KEY (NRIC),
	CONSTRAINT UQ_User_Email UNIQUE (Email)
)
GO

/****** Object:  Table BankAccount ******/
CREATE TABLE BankAccount (
	AccNo VARCHAR(20) NOT NULL,
	Balance smallmoney NOT NULL,
	NRIC Char(9) NOT NULL,
	CONSTRAINT PK_BankAccount PRIMARY KEY (AccNo),
	CONSTRAINT FK_BankAccount_NRIC FOREIGN KEY (NRIC) REFERENCES BankUser (NRIC)
)
GO

/****** Object:  Table FutureTransfer  ******/
CREATE TABLE FutureTransfer (
	FutureID int IDENTITY (1,1),
	Recipient varchar (20) NOT NULL,
	Sender varchar (20) NOT NULL,
	Amount smallmoney NOT NULL,
	PlanTime DATETIME NOT NULL,
	Notified char(1) CHECK (Notified = 'T' OR Notified = 'F') NOT NULL,
	Completed char(1) CHECK (Completed = 'T' OR Completed = 'F') NOT NULL,
	CONSTRAINT PK_FutureTransfer PRIMARY KEY (FutureID),
	CONSTRAINT FK_FutureTransfer_Recepient FOREIGN KEY (Recipient) REFERENCES BankAccount (AccNo),
	CONSTRAINT FK_FutureTransfer_Sender FOREIGN KEY (Sender) REFERENCES BankAccount (AccNo)
)
GO

/****** Object:  Table Transaction  ******/
CREATE TABLE Transactions (
	TransacID int IDENTITY (1,1),
	Recipient varchar (20) NOT NULL,
	Sender varchar (20) NOT NULL,
	Amount smallmoney NOT NULL,
	TimeTransfer DATETIME NOT NULL,
	Notified char(1) CHECK (Notified = 'T' OR Notified = 'F') NOT NULL,
	Completed char(1) CHECK (Completed = 'T' OR Completed = 'F') NOT NULL,
	Type char(10) CHECK (Type = 'Immediate' OR Type = 'Future') NOT NULL
	CONSTRAINT PK_Transactions PRIMARY KEY (TransacID),
	CONSTRAINT FK_Transactions_Recepient FOREIGN KEY (Recipient) REFERENCES BankAccount (AccNo),
	CONSTRAINT FK_Transactions_Sender FOREIGN KEY (Sender) REFERENCES BankAccount (AccNo)
)
GO

/*****  Create records in Category Table *****/
insert into BankUser (NRIC, Email, Phone, Name, TransLimit, Password) 
values ('T1234567A', 'abc@mail.com', '98765432','Lim Seng Benny',5000, 'limsngbnny32')
insert into BankUser (NRIC, Email, Phone, Name, TransLimit, Password)
values ('T2345678B', 'xyz@mail.com', '87654321', 'John Tan Wei Lang',7500, 'jtwenglai44')
insert into BankUser (NRIC, Email, Phone, Name, TransLimit, Password)
values ('T3456789C', 'def@mail.com', '97654321', 'Jenny Teo',5000, 'teojenj10')
GO

/*****  Create records in Publisher Table *****/
insert into BankAccount (AccNo, Balance, NRIC) 
values ('123-456789-012', 100000, 'T1234567A')
insert into BankAccount (AccNo, Balance, NRIC)
values ('234-567829-901', 150000, 'T2345678B')
insert into BankAccount (AccNo, Balance, NRIC)
values ('124-509877-612', 120000, 'T3456789C')
GO

/*****  Create records in FutureTransfer Table *****/
insert into FutureTransfer (Recipient, Sender, Amount, PlanTime, Notified, Completed)
values ('123-456789-012','234-567829-901', 3000, '2021-12-11 13:00', 'F', 'F')

/*****  Create records in Transaction Table *****/
insert into Transactions(Recipient, Sender, Amount, TimeTransfer, Notified, Completed, Type)
values ('234-567829-901','123-456789-012', 2500, '2021-11-01 16:30', 'T', 'T', 'Immediate')
insert into Transactions(Recipient, Sender, Amount, TimeTransfer, Notified, Completed, Type)
values ('123-456789-012','234-567829-901', 3000, '2021-12-11 13:00', 'F', 'F','Future')