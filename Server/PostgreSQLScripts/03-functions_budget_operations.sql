--Get by id
CREATE OR REPLACE FUNCTION budget_operations_getById(id INT)
RETURNS TABLE(
	operation_id INT,
	operation_date TIMESTAMP WITH TIME ZONE,
	operation_type INT,
	operation_sum NUMERIC,
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
		EXECUTE format (
			'SELECT
				budget_operations.id, budget_operations.date, budget_operations.type, budget_operations.sum, budget_operations.item_id, budget_items.name, budget_items.type
			FROM
				budget_operations LEFT JOIN budget_items ON budget_operations.item_id = budget_items.id
			WHERE
				budget_operations.id = %s',
			id);
END $$;

--Get all with paging for period excluding end
CREATE OR REPLACE FUNCTION budget_operations_getAllForPeriodExcludingEndWithPaging(types INT[], dateFrom TIMESTAMP WITH TIME ZONE, dateTo TIMESTAMP WITH TIME ZONE, page_number INT, page_size INT)
RETURNS TABLE(
	operation_id INT,
	operation_date TIMESTAMP WITH TIME ZONE,
	operation_type INT,
	operation_sum NUMERIC,
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
		EXECUTE format (
			'SELECT
				budget_operations.id, budget_operations.date, budget_operations.type, budget_operations.sum, budget_operations.item_id, budget_items.name, budget_items.type
			FROM
				budget_operations LEFT JOIN budget_items ON budget_operations.item_id = budget_items.id
			WHERE
				budget_operations.type = ANY (%L::int[]) AND budget_operations.date >= %L AND budget_operations.date <= %L
			ORDER BY
				budget_operations.date
			LIMIT
				%s
			OFFSET
				%s',
			types, dateFrom, dateTo, page_size, page_size * (page_number - 1));
END $$;

--Post
CREATE OR REPLACE FUNCTION budget_operations_post(date TIMESTAMP WITH TIME ZONE, type INT, sum DOUBLE PRECISION, item INT)
RETURNS TABLE(
	operation_id INT,
	operation_date TIMESTAMP WITH TIME ZONE,
	operation_type INT,
	operation_sum NUMERIC,
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'INSERT INTO budget_operations (date, type, sum, item_id) VALUES (%L, %L, %L, %L) RETURNING id',
		date, type, sum, item) INTO result_id;
	IF result_id IS NULL THEN
		result_id = 0;
	END IF;
	RETURN QUERY
		EXECUTE format(
			'SELECT
				budget_operations.id, budget_operations.date, budget_operations.type, budget_operations.sum, budget_operations.item_id, budget_items.name, budget_items.type
			FROM
				budget_operations LEFT JOIN budget_items ON budget_operations.item_id = budget_items.id
			WHERE
				budget_operations.id = %s',
			result_id);
END $$;

--Put
CREATE OR REPLACE FUNCTION budget_operations_put(id INT, date TIMESTAMP WITH TIME ZONE, sum DOUBLE PRECISION, item INT)
RETURNS TABLE (
	operation_id INT,
	operation_date TIMESTAMP WITH TIME ZONE,
	operation_type INT,
	operation_sum NUMERIC,
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'UPDATE budget_operations SET date = %L, sum = %L, item_id = %L WHERE id = %s RETURNING id',
		date, sum, item, id) INTO result_id;
	IF result_id IS NULL THEN
		result_id = 0;
	END IF;
	RETURN QUERY
		EXECUTE format(
			'SELECT
				budget_operations.id, budget_operations.date, budget_operations.type, budget_operations.sum, budget_operations.item_id, budget_items.name, budget_items.type
			FROM
				budget_operations LEFT JOIN budget_items ON budget_operations.item_id = budget_items.id
			WHERE
				budget_operations.id = %s',
			result_id);
END $$;

--Delete
CREATE OR REPLACE FUNCTION budget_operations_delete(id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'DELETE FROM budget_operations WHERE id = %s RETURNING id',
		id) INTO result_id;
	RETURN result_id IS NOT NULL;
END $$;
