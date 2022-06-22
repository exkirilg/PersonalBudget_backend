--Budget items
CREATE TABLE budget_items (
	id SERIAL PRIMARY KEY,
	name VARCHAR(100) NOT NULL,
	type INT NOT NULL
);

--Budget operations
CREATE TABLE budget_operations (
	id SERIAL PRIMARY KEY,
	date TIMESTAMP WITH TIME ZONE NOT NULL,
	type INT NOT NULL,
	item_id SERIAL NOT NULL REFERENCES budget_items,
	sum NUMERIC(15, 2)
);

--Seeding items
INSERT INTO budget_items (name, type) VALUES
	('Salary', 0),
	('Deposit interest', 0),
	('Rent', 1),
	('Groceries', 1),
	('Utility bills', 1),
	('Entertaiments', 1),
	('Misc', 1);

--Seeding operations
INSERT INTO budget_operations (date, type, item_id, sum) VALUES
	('2022-06-01T09:00:00Z', 1, 4, 4700.27),
	('2022-06-01T16:00:00Z', 0, 1, 40000),
	('2022-06-08T09:00:00Z', 1, 4, 3200.15),
	('2022-06-15T09:00:00Z', 1, 4, 5650.1),
	('2022-06-16T12:00:00Z', 0, 1, 40000),
	('2022-06-22T09:00:00Z', 1, 4, 4899.8),
	('2022-06-25T15:00:00Z', 1, 5, 6329.15),
	('2022-06-25T15:00:00Z', 1, 3, 25000),
	('2022-06-27T12:00:00Z', 0, 2, 1753.21),
	('2022-06-29T09:00:00Z', 1, 4, 3733.24),
	('2022-06-30T20:59:59Z', 1, 6, 11250),
	('2022-06-30T20:59:59Z', 1, 7, 5700);
